using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using LetsGame.Web.Data;
using LetsGame.Web.Infrastructure.AspNet;
using LetsGame.Web.Services;
using LetsGame.Web.Services.Igdb;
using LetsGame.Web.Services.Itad;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options
                    .UseNpgsql(Configuration.GetConnectionString("Postgres"))
                    // .UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
                    .LogTo(Console.WriteLine, new[] {DbLoggerCategory.Database.Command.Name}, LogLevel.Information));
            
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDataProtection()
                .PersistKeysToDbContext<ApplicationDbContext>()
                .SetApplicationName("LetsGame");
            
            services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    var config = Configuration.GetSection("Authentication:Google");

                    options.ClientId = config["ClientId"];
                    options.ClientSecret = config["ClientSecret"];
                });
            
            services.AddRazorPages();

            services.Configure<ItadOptions>(Configuration.GetSection("itad"));
            services.Configure<IgdbOptions>(Configuration.GetSection("igdb"));
            services.AddHttpClient<ItadClient>(ItadClient.Configure);
            services.AddHttpClient<IgdbClient>(IgdbClient.Configure);
            
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            
            services.Replace(ServiceDescriptor.Singleton<IHtmlGenerator, CustomHtmlGenerator>());

            services.AddScoped<SlugGenerator>();
            services.AddScoped<GroupService>();
            services.AddScoped<DateService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapRazorPages(); });
        }
    }
}