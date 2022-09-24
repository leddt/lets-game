using System;
using System.Net;
using System.Text;
using HotChocolate;
using HotChocolate.Types.NodaTime;
using LetsGame.Web.Authorization;
using LetsGame.Web.Authorization.Requirements;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using LetsGame.Web.Data;
using LetsGame.Web.GraphQL;
using LetsGame.Web.Hubs;
using LetsGame.Web.Infrastructure.AspNet;
using LetsGame.Web.RecurringTasks;
using LetsGame.Web.Services;
using LetsGame.Web.Services.Igdb;
using LetsGame.Web.Services.Itad;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using WebPush;

namespace LetsGame.Web
{
    public class Startup
    {
        private readonly IWebHostEnvironment env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            this.env = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var databaseUrl = Configuration["DATABASE_URL"];
            if (string.IsNullOrWhiteSpace(databaseUrl))
            {
                services.AddHostedService<EmbeddedPostgresHostedService>();
                databaseUrl = EmbeddedPostgresHostedService.DatabaseUrl;
            }

            services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
            {
                options
                    .UseNpgsql(ConvertPostgresqlConnectionString(databaseUrl))
                    .LogTo(Console.WriteLine, new[] {DbLoggerCategory.Database.Command.Name}, LogLevel.Information);
            });
            services.AddDbContextPool<ApplicationDbContext>(options =>
            {
                options
                    .UseNpgsql(ConvertPostgresqlConnectionString(databaseUrl))
                    .LogTo(Console.WriteLine, new[] {DbLoggerCategory.Database.Command.Name}, LogLevel.Information);
            });
            
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDataProtection()
                .PersistKeysToDbContext<ApplicationDbContext>()
                .SetApplicationName("LetsGame");
            
            services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            var googleConfig = Configuration.GetSection("Authentication:Google");

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

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("::ffff:172.0.0.0"), 104));
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                           ForwardedHeaders.XForwardedProto;
            });
            
            services.AddRazorPages();

            services.Configure<ItadOptions>(Configuration.GetSection("itad"));
            services.Configure<IgdbOptions>(Configuration.GetSection("igdb"));
            services.Configure<SendGridOptions>(Configuration.GetSection("SendGrid"));
            
            services.AddHttpClient<ItadClient>(ItadClient.Configure);
            services.AddHttpClient<IGameSearcher, IgdbClient>(IgdbClient.Configure);
            
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            
            services.Replace(ServiceDescriptor.Singleton<IHtmlGenerator, CustomHtmlGenerator>());

            services.AddTransient<SlugGenerator>();
            services.AddTransient<GroupService>();
            services.AddTransient<DateService>();
            services.AddTransient<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();

            services.AddTransient(_ => DateTimeZoneProviders.Bcl);
            
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<INotificationService, NotificationService>();

            services.AddSingleton(new VapidDetails(
                Configuration["vapid:subject"],
                Configuration["vapid:publicKey"],
                Configuration["vapid:privateKey"]));
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
                .ConfigureSchema(x => x.AddType<LocalDateTimeType>());
            
            // SPA Services
            services.AddSpaStaticFiles(options =>
            {
                options.RootPath = "ClientApp";
            });
            
            // SignalR
            services.AddSignalR();
        }

        private string ConvertPostgresqlConnectionString(string uriString)
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ApplicationDbContext db)
        {
            using (db)
            {
                db.Database.Migrate();
            }
            
            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Lax
            });

            // For now we'll rely on the hosting platform to redirect to https 
            // app.UseHttpsRedirection();
            
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseWebSockets();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL();
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHub<PresenceHub>("/presences");
            });

            // Force login before going to SPA
            app.Use(async (context, next) =>
            {
                if (context.User.Identity?.IsAuthenticated != true)
                    await context.ChallengeAsync();
                else
                    await next();
            });
            
            app.UseSpaStaticFiles();
            app.UseSpa(spa =>
            {
                if (env.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer(Configuration["FrontendDevServer"]);
                }
            });
        }
    }
}