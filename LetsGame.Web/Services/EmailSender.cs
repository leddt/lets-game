using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace LetsGame.Web.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IOptions<SendGridOptions> _sendgridOptions;

        public EmailSender(IOptions<SendGridOptions> sendgridOptions)
        {
            _sendgridOptions = sendgridOptions;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(_sendgridOptions.Value.ApiKey)) return Task.CompletedTask;
            
            var message = new SendGridMessage
            {
                From = new EmailAddress("letsgame-noreply@leddt.com", "Let's Game!"),
                Subject = subject,
                HtmlContent = htmlMessage
            };
            
            message.AddTo(email);
            message.SetClickTracking(false, false);
            
            var client = new SendGridClient(_sendgridOptions.Value.ApiKey);
            return client.SendEmailAsync(message);
        }
    }

    public class SendGridOptions
    {
        public string ApiKey { get; set; }
    }
}