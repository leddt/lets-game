using System;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types.NodaTime;
using LetsGame.Web.Authorization;
using LetsGame.Web.Authorization.Requirements;
using LetsGame.Web.Data;
using LetsGame.Web.GraphQL;
using LetsGame.Web.Hubs;
using LetsGame.Web.Infrastructure.AspNet;
using LetsGame.Web.RecurringTasks;
using LetsGame.Web.Services;
using LetsGame.Web.Services.Igdb;
using LetsGame.Web.Services.Itad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using WebPush;

namespace LetsGame.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationDatabase(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = GetPostgresConnectionString();
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseNodaTime();
        var dataSource = dataSourceBuilder.Build();
            
        services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
        {
            options
                .UseNpgsql(dataSource, o => o
                    .UseNodaTime()
                    .MigrationsHistoryTable("__EFMigrationsHistory", "private"))
                .LogTo(Console.WriteLine, [DbLoggerCategory.Database.Command.Name], LogLevel.Information);
        });
        services.AddDbContextPool<ApplicationDbContext>(options =>
        {
            options
                .UseNpgsql(dataSource, o => o
                    .UseNodaTime()
                    .MigrationsHistoryTable("__EFMigrationsHistory", "private"))
                .LogTo(Console.WriteLine, [DbLoggerCategory.Database.Command.Name], LogLevel.Information);
        });
        
        return services;

        string GetPostgresConnectionString()
        {
            var databaseUrl = configuration["DATABASE_URL"];
            if (!string.IsNullOrWhiteSpace(databaseUrl))
                return ConvertPostgresUrlToConnectionString(databaseUrl);
            
            var result = configuration.GetConnectionString("db");
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

    public static IServiceCollection AddApplicationAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var googleConfig = configuration.GetSection("Authentication:Google");

        var auth = services
            .AddAuthentication()
            .AddSteam();
            
        if (!string.IsNullOrWhiteSpace(googleConfig["ClientId"]))
        {
            auth
                .AddGoogle(options =>
                {
                    options.ClientId = googleConfig["ClientId"];
                    options.ClientSecret = googleConfig["ClientSecret"];
                });
        }

        services.AddAuthorization(x =>
        {
            x.AddPolicy(AuthPolicies.ReadGroup, policy => policy.AddRequirements(new AccessGroupRequirement(asOwner: false)));
            x.AddPolicy(AuthPolicies.ManageGroup, policy => policy.AddRequirements(new AccessGroupRequirement(asOwner: true)));
            x.AddPolicy(AuthPolicies.ReadSession, policy => policy.AddRequirements(new AccessSessionRequirement(manage: false)));
            x.AddPolicy(AuthPolicies.ManageSession, policy => policy.AddRequirements(new AccessSessionRequirement(manage: true)));
            x.AddPolicy(AuthPolicies.ReadSlot, policy => policy.AddRequirements(new AccessSlotRequirement(manage: false)));
            x.AddPolicy(AuthPolicies.ManageSlot, policy => policy.AddRequirements(new AccessSlotRequirement(manage: true)));
        });
        services.AddTransient<IAuthorizationHandler, AccessGroupRequirementHandler>();
        services.AddTransient<IAuthorizationHandler, AccessSessionRequirementHandler>();
        services.AddTransient<IAuthorizationHandler, AccessSlotRequirementHandler>();

        return services;
    }
    
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // IsThereAnyDeal
        services.Configure<ItadOptions>(configuration.GetSection("itad"));
        services.AddHttpClient<ItadClient>(ItadClient.Configure);
        
        services.Configure<IgdbOptions>(configuration.GetSection("igdb"));
        services.AddHttpClient<IGameSearcher, IgdbClient>(IgdbClient.Configure);

        services.AddTransient<SlugGenerator>();
        services.AddTransient<GroupService>();
        services.AddTransient<DateService>();
        services.AddTransient<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
        
        services.AddTransient<IEmailSender, EmailSender>();
        services.AddTransient<INotificationService, NotificationService>();

        services.AddSingleton(new VapidDetails(
            configuration["vapid:subject"],
            configuration["vapid:publicKey"],
            configuration["vapid:privateKey"]));
        services.AddTransient<WebPushClient>();
        services.AddTransient<IPushSender, PushSender>();
        
        services.AddSingleton<PresenceCache>();

        // Recurring tasks
        services.AddTransient<RecurringTaskRunner>();
        services.AddTransient<IRecurringTask, SendEventStartingSoonNotifications>();
        services.AddTransient<IRecurringTask, CleanUpPastEvents>();
            
        // GraphQL
        services
            .AddGraphQLServer()
            .AddAuthorization()
            .AddQueryType<Query>()
            .AddMutationType<Mutation>()
            .AddSubscriptionType<Subscription>()
            .AddInMemorySubscriptions()
            .RegisterDbContext<ApplicationDbContext>(DbContextKind.Pooled)
            .ConfigureSchema(x => x.AddType<LocalDateTimeType>());
        
        return services;
    }
}