﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace LetsGame.Web.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IOptions<SendGridOptions> _sendgridOptions;
        private readonly IConfiguration _config;

        public EmailSender(IOptions<SendGridOptions> sendgridOptions, IConfiguration config)
        {
            _sendgridOptions = sendgridOptions;
            _config = config;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(_sendgridOptions.Value.ApiKey))
            {
                Console.WriteLine($"Not sending email because SendGrid is not configured.");
                return Task.CompletedTask;
            }
            
            var message = new SendGridMessage
            {
                From = new EmailAddress(_config["EmailFrom"], "Let's Game!"),
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