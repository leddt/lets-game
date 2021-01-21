using System;
using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.RecurringTasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
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
            var services = scope.ServiceProvider;

            var config = services.GetRequiredService<IConfiguration>();
            var mailer = services.GetRequiredService<IEmailSender>();

            var errorReportEmail = config["ErrorReportEmail"];
            
            var tasks = services.GetServices<IRecurringTask>();
            foreach (var task in tasks)
            {
                var taskName = task.GetType().Name;
                Console.WriteLine("Running task {0}...", taskName);

                try
                {
                    await task.Run();
                    Console.WriteLine("Task {0} completed", taskName);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Task {0} failed", taskName);
                    Console.Error.WriteLine(ex);

                    await TryReportError(mailer, errorReportEmail, taskName, ex);
                }
            }
        }

        private static async Task TryReportError(
            IEmailSender mailer, 
            string email, 
            string taskName,
            Exception exception)
        {
            if (string.IsNullOrWhiteSpace(email)) return;

            try
            {
                await mailer.SendEmailAsync(
                    email,
                    "[Let's Game!] Error processing scheduled task",
                    $"<p>There was an error during the task {taskName}.</p>" +
                    $"<pre>${exception}</pre>");
            }
            catch (Exception reportException)
            {
                Console.Error.WriteLine("Failed to send error notification");
                Console.Error.WriteLine(reportException);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}