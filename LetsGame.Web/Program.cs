using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LetsGame.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
            if (args.Contains("console")) 
                RunConsole(host);
            else
                RunWeb(host);
        }

        private static void RunConsole(IHost host)
        {
            Console.WriteLine("Running as console");

            using var scope = host.Services.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var groups = db.Groups.ToList();
            foreach (var g in groups)
                Console.WriteLine("- {0}", g.Name);
        }

        private static void RunWeb(IHost host) => host.Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}