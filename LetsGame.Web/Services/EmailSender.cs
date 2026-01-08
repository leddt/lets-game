using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace LetsGame.Web.Services
{
    public class EmailSender(IOptions<SendGridOptions> sendgridOptions, IConfiguration config)
        : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(sendgridOptions.Value.ApiKey))
            {
                Console.WriteLine("Not sending email because SendGrid is not configured.");
                return Task.CompletedTask;
            }
            
            var message = new SendGridMessage
            {
                From = new EmailAddress(config["EmailFrom"], "Let's Game!"),
                Subject = subject,
                HtmlContent = htmlMessage
            };
            
            message.AddTo(email);
            message.SetClickTracking(false, false);
            
            var client = new SendGridClient(sendgridOptions.Value.ApiKey);
            return client.SendEmailAsync(message);
        }
    }

    public class SendGridOptions
    {
        public string? ApiKey { get; set; }
    }
}