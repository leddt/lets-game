using System;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using LetsGame.Web.Data;
using LetsGame.Web.Infrastructure.AspNet;
using LetsGame.Web.RecurringTasks;
using LetsGame.Web.Services;
using LetsGame.Web.Services.Igdb;
using LetsGame.Web.Services.Itad;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            
            services.AddDbContext<ApplicationDbContext>(options =>
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
            
            var auth = services.AddAuthentication();
            if (!string.IsNullOrWhiteSpace(googleConfig["ClientId"]))
            {
                auth
                    .AddGoogle(options =>
                    {
                        options.ClientId = googleConfig["ClientId"];
                        options.ClientSecret = googleConfig["ClientSecret"];
                    });
            }

            services.AddRazorPages();

            services.Configure<ItadOptions>(Configuration.GetSection("itad"));
            services.Configure<IgdbOptions>(Configuration.GetSection("igdb"));
            services.Configure<SendGridOptions>(Configuration.GetSection("SendGrid"));
            
            services.AddHttpClient<ItadClient>(ItadClient.Configure);
            services.AddHttpClient<IgdbClient>(IgdbClient.Configure);
            
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            
            services.Replace(ServiceDescriptor.Singleton<IHtmlGenerator, CustomHtmlGenerator>());

            services.AddScoped<SlugGenerator>();
            services.AddScoped<GroupService>();
            services.AddScoped<DateService>();
            services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
            
            services.AddTransient<IEmailSender, EmailSender>();

            // Recurring tasks
            services.AddTransient<IRecurringTask, TestRecurringTask>();
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

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapRazorPages(); });
        }
    }
}