using System;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using LetsGame.Web.Data;
using LetsGame.Web.Extensions;
using LetsGame.Web.Hubs;
using LetsGame.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NodaTime;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

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

            services.AddApplicationDatabase(Configuration);
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDataProtection()
                .PersistKeysToDbContext<ApplicationDbContext>()
                .SetApplicationName("LetsGame");
            
            services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddApplicationAuth(Configuration);

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("::ffff:169.0.0.0"), 104));
                options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("::ffff:172.0.0.0"), 104));
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                           ForwardedHeaders.XForwardedProto;
            });
            
            services.AddRazorPages();

            services.AddApplicationServices(Configuration);
            services.Configure<SendGridOptions>(Configuration.GetSection("SendGrid"));
            
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddTransient(_ => DateTimeZoneProviders.Bcl);
            
            // SPA Services
            services.AddSpaStaticFiles(options =>
            {
                options.RootPath = "ClientApp";
            });
            
            // SignalR
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ApplicationDbContext db)
        {
            using (db)
            {
                db.Database.Migrate();
            }

            app.Use(async (ctx, next) =>
            {
                Console.WriteLine("Request IP: {0}", ctx.Connection.RemoteIpAddress);
                await next();
            });
            
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