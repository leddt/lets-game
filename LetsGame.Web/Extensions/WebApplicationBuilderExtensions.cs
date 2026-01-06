using System;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types.NodaTime;
using LetsGame.Web.Authorization;
using LetsGame.Web.Authorization.Requirements;
using LetsGame.Web.Data;
using LetsGame.Web.GraphQL;
using LetsGame.Web.Hubs;
using LetsGame.Web.RecurringTasks;
using LetsGame.Web.Services;
using LetsGame.Web.Services.Igdb;
using LetsGame.Web.Services.Itad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using WebPush;

namespace LetsGame.Web.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddApplicationDatabase(this WebApplicationBuilder builder)
    {
        var connectionString = GetPostgresConnectionString();
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseNodaTime();
        var dataSource = dataSourceBuilder.Build();
            
        builder.Services
            .AddPooledDbContextFactory<ApplicationDbContext>(x => ConfigureDbContext(x, dataSource))
            .AddDbContextPool<ApplicationDbContext>(x => ConfigureDbContext(x, dataSource));
        
        builder.EnrichNpgsqlDbContext<ApplicationDbContext>();
        
        return builder;

        string GetPostgresConnectionString()
        {
            var databaseUrl = builder.Configuration["DATABASE_URL"];
            if (!string.IsNullOrWhiteSpace(databaseUrl))
                return ConvertPostgresUrlToConnectionString(databaseUrl);
            
            var result = builder.Configuration.GetConnectionString("db");
            if (!string.IsNullOrWhiteSpace(result))
                return result;
            
            throw new InvalidOperationException("No db connection string is configured");
        }
        
        string ConvertPostgresUrlToConnectionString(string uriString)
        {
            var uri = new Uri(uriString);
            var userInfo = uri.UserInfo.Split(':');

            var cs = $"Host={uri.Host};" +
                     $"Port={uri.Port};" +
                     $"Database={uri.AbsolutePath.Trim('/')};" +
                     $"Username={userInfo[0]};" +
                     $"Password={userInfo[1]};" +
                     $"SSL Mode=Prefer;" +
                     $"Trust Server Certificate=true;";
            
            return cs;
        }
    }

    public static void ConfigureDbContext(DbContextOptionsBuilder options, NpgsqlDataSource dataSource = null)
    {
        if (dataSource is not null)
            options.UseNpgsql(dataSource, ConfigureNpgsql);
        else
            options.UseNpgsql(ConfigureNpgsql);

        options.ConfigureWarnings(w => w.Log(RelationalEventId.PendingModelChangesWarning));
        
        return;

        void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder npgsqlOptions)
        {
            npgsqlOptions
                .UseNodaTime()
                .MigrationsHistoryTable("__EFMigrationsHistory", "private");
        }
    }

    public static WebApplicationBuilder AddApplicationAuth(
        this WebApplicationBuilder builder)
    {
        var googleConfig = builder.Configuration.GetSection("Authentication:Google");

        var auth = builder.Services
            .AddAuthentication()
            .AddSteam();
            
        if (!string.IsNullOrWhiteSpace(googleConfig["ClientId"]) &&
            !string.IsNullOrWhiteSpace(googleConfig["ClientSecret"]))
        {
            auth
                .AddGoogle(options =>
                {
                    options.ClientId = googleConfig["ClientId"];
                    options.ClientSecret = googleConfig["ClientSecret"];
                });
        }

        builder.Services
            .AddAuthorization(x =>
            {
                x.AddPolicy(AuthPolicies.ReadGroup, policy => policy.AddRequirements(new AccessGroupRequirement(asOwner: false)));
                x.AddPolicy(AuthPolicies.ManageGroup, policy => policy.AddRequirements(new AccessGroupRequirement(asOwner: true)));
                x.AddPolicy(AuthPolicies.ReadSession, policy => policy.AddRequirements(new AccessSessionRequirement(manage: false)));
                x.AddPolicy(AuthPolicies.ManageSession, policy => policy.AddRequirements(new AccessSessionRequirement(manage: true)));
                x.AddPolicy(AuthPolicies.ReadSlot, policy => policy.AddRequirements(new AccessSlotRequirement(manage: false)));
                x.AddPolicy(AuthPolicies.ManageSlot, policy => policy.AddRequirements(new AccessSlotRequirement(manage: true)));
            })
            .AddTransient<IAuthorizationHandler, AccessGroupRequirementHandler>()
            .AddTransient<IAuthorizationHandler, AccessSessionRequirementHandler>()
            .AddTransient<IAuthorizationHandler, AccessSlotRequirementHandler>();

        return builder;
    }
    
    public static WebApplicationBuilder AddApplicationServices(
        this WebApplicationBuilder builder
    )
    {
        // IsThereAnyDeal
        builder.Services
            .Configure<ItadOptions>(builder.Configuration.GetSection("itad"))
            .AddHttpClient<ItadClient>(ItadClient.Configure);
        
        builder.Services
            .Configure<IgdbOptions>(builder.Configuration.GetSection("igdb"))
            .AddHttpClient<IGameSearcher, IgdbClient>(IgdbClient.Configure);

        builder.Services
            .AddTransient<SlugGenerator>()
            .AddTransient<GroupService>()
            .AddTransient<DateService>()
            .AddTransient<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
        
        builder.Services
            .AddTransient<IEmailSender, EmailSender>()
            .AddTransient<INotificationService, NotificationService>();

        builder.Services
            .AddSingleton(new VapidDetails(
                builder.Configuration["vapid:subject"],
                builder.Configuration["vapid:publicKey"],
                builder.Configuration["vapid:privateKey"]))
            .AddTransient<WebPushClient>()
            .AddTransient<IPushSender, PushSender>();
        
        builder.Services.AddSingleton<PresenceCache>();

        // Recurring tasks
        builder.Services
            .AddTransient<RecurringTaskRunner>()
            .AddTransient<IRecurringTask, SendEventStartingSoonNotifications>()
            .AddTransient<IRecurringTask, CleanUpPastEvents>();
            
        // GraphQL
        builder.Services
            .AddGraphQLServer()
            .AddAuthorization()
            .AddQueryType<Query>()
            .AddMutationType<Mutation>()
            .AddSubscriptionType<Subscription>()
            .AddInMemorySubscriptions()
            .RegisterDbContextFactory<ApplicationDbContext>()
            .AddWebTypes()
            .ConfigureSchema(x => x.AddType<LocalDateTimeType>());
        
        // SendGrid
        builder.Services
            .Configure<SendGridOptions>(builder.Configuration.GetSection("SendGrid"));
        
        // NodaTime
        builder.Services.AddTransient(_ => DateTimeZoneProviders.Bcl);
        
        return builder;
    }
}