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
            using var scope = host.Services.CreateScope();

            var runner = scope.ServiceProvider.GetRequiredService<RecurringTaskRunner>();
            await runner.RunAll();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}