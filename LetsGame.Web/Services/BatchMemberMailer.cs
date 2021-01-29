using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace LetsGame.Web.Services
{
    public class BatchMemberMailer
    {
        private readonly ICurrentUserAccessor _currentUserAccessor;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;

        public BatchMemberMailer(ICurrentUserAccessor currentUserAccessor, UserManager<AppUser> userManager, IEmailSender emailSender)
        {
            _currentUserAccessor = currentUserAccessor;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public async Task EmailMembersAsync(
            IEnumerable<Membership> members, 
            string subject, 
            Func<Membership, string> getMessage, 
            Func<AppUser, bool> isUnsubscribed)
        {
            var currentUser = _currentUserAccessor.CurrentUser;
            var currentUserId = currentUser == null 
                ? null 
                : _userManager.GetUserId(currentUser);

            var skipped = 0;
            
            foreach (var member in members)
            {
                if (member.UserId == currentUserId)
                {
                    Console.WriteLine("Skipped sending email to current user");
                    continue;
                }
                if (isUnsubscribed(member.User))
                {
                    skipped++;
                    continue;
                }

                await _emailSender.SendEmailAsync(
                    member.User.Email,
                    subject,
                    getMessage(member));
            }

            if (skipped > 0)
            {
                Console.WriteLine("Skipped sending email to {0} unsubscribed users", skipped);
            }
        }
    }
}