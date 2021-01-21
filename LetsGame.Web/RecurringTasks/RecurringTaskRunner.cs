using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;

namespace LetsGame.Web.RecurringTasks
{
    public class RecurringTaskRunner
    {
        private readonly IConfiguration _config;
        private readonly IEmailSender _mailer;
        private readonly IEnumerable<IRecurringTask> _recurringTasks;

        public RecurringTaskRunner(IConfiguration config, IEmailSender mailer, IEnumerable<IRecurringTask> recurringTasks)
        {
            _config = config;
            _mailer = mailer;
            _recurringTasks = recurringTasks;
        }

        public async Task RunAll()
        {
            Console.WriteLine("Running recurring tasks");
            
            foreach (var task in _recurringTasks)
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

                    await TryReportError(taskName, ex);
                }
            }
        }

        private async Task TryReportError(
            string taskName,
            Exception exception)
        {
            var errorReportEmail = _config["ErrorReportEmail"];
            if (string.IsNullOrWhiteSpace(errorReportEmail)) return;

            try
            {
                await _mailer.SendEmailAsync(
                    errorReportEmail,
                    "[Let's Game!] Error processing scheduled task",
                    $"<p>There was an error during the task {taskName}.</p>" +
                    $"<pre>{exception}</pre>");
            }
            catch (Exception reportException)
            {
                Console.Error.WriteLine("Failed to send error notification");
                Console.Error.WriteLine(reportException);
            }
        }
    }
}