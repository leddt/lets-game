using System;
using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.RecurringTasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LetsGame.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
            if (args.Contains("run-tasks")) 
                await RunTasks(host);
            else
                await host.RunAsync();
        }

        private static async Task RunTasks(IHost host)
        {
            Console.WriteLine("Running recurring tasks");

            using var scope = host.Services.CreateScope();
            
            var tasks = scope.ServiceProvider.GetServices<IRecurringTask>();
            foreach (var task in tasks)
                await task.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}