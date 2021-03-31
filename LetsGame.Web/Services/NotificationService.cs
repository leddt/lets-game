using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.Services
{
    public interface INotificationService
    {
        Task NotifyEventAdded(GroupEvent newEvent);
        Task NotifyEventStartingSoon(GroupEvent ev);
        Task SendEventReminderAsync(long eventId);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;
        private readonly DateService _dateService;
        private readonly GroupService _groupService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserAccessor _currentUserAccessor;

        public NotificationService(
            ApplicationDbContext db, 
            DateService dateService,
            GroupService groupService,
            UserManager<AppUser> userManager,
            IEmailSender emailSender,
            IUrlHelperFactory urlHelperFactory, 
            IActionContextAccessor actionContextAccessor,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserAccessor currentUserAccessor)
        {
            _db = db;
            _dateService = dateService;
            _groupService = groupService;
            _userManager = userManager;
            _emailSender = emailSender;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _httpContextAccessor = httpContextAccessor;
            _currentUserAccessor = currentUserAccessor;
        }

        public async Task NotifyEventAdded(GroupEvent newEvent)
        {
            var allGroupMembers = await _db.Memberships
                .Include(x => x.User)
                .Where(x => x.GroupId == newEvent.GroupId)
                .ToListAsync();

            var creator = allGroupMembers.First(x => x.UserId == newEvent.CreatorId);
            var game = await _db.GroupGames.FindAsync(newEvent.GameId);
            var group = await _db.Groups.FindAsync(newEvent.GroupId);
            var slotsUtc = newEvent.Slots.Select(x => x.ProposedDateAndTimeUtc);
            var groupUrl = GetGroupUrl(group);

            await EmailMembersAsync(
                allGroupMembers.Where(x => x.UserId != newEvent.CreatorId), 
                $"A new session has been proposed in {group.Name}", 
                GetEmailMessage,
                x => x.UnsubscribeNewEvent);

            string GetEmailMessage(Membership member)
            {
                return $"<p>Hi {HtmlEncode(member.DisplayName)}!" +
                       $"<p>A new session for <strong>{(game == null ? "any game" : HtmlEncode(game.Name))}</strong> has been proposed by <strong>{HtmlEncode(creator.DisplayName)}</strong>." +
                       $"<p>The proposed time slots are:" +
                       $"<ul>" +
                       string.Join("", slotsUtc.Select(x => $"<li>{HtmlEncode(_dateService.FormatUtcToUserFriendlyDate(x, member.User))}")) +
                       $"</ul>" +
                       $"<p><a href=\"{groupUrl}\">Go to your group's page to vote on it!</a>";
            }
        }
        
        public async Task NotifyEventStartingSoon(GroupEvent ev)
        {
            // Sanity checks
            if (ev.ChosenDateAndTimeUtc == null) return;

            var eventSlot = await _db.GroupEventSlots
                .Include(x => x.Votes)
                .Where(x => x.EventId == ev.Id)
                .Where(x => x.ProposedDateAndTimeUtc == ev.ChosenDateAndTimeUtc)
                .FirstOrDefaultAsync();
            if (eventSlot == null) return;
            
            var participants = await _db.Memberships
                .Include(x => x.User)
                .Where(m => eventSlot.Votes.Select(v => v.VoterId).Contains(m.UserId))
                .Where(x => x.GroupId == ev.GroupId)
                .OrderBy(x => x.DisplayName)
                .ToListAsync();

            var group = await _db.Groups.FindAsync(ev.GroupId);
            var game = await _db.GroupGames.FindAsync(ev.GameId);

            await EmailMembersAsync(
                participants,
                $"A session is starting soon in {group.Name}",
                GetEmailMessage,
                x => x.UnsubscribeEventReminder);
            
            string GetEmailMessage(Membership member)
            {
                var friendlyTime = _dateService.FormatUtcToUserFriendlyDate(ev.ChosenDateAndTimeUtc.Value, member.User);

                return $"<p>Hi {HtmlEncode(member.DisplayName)}!" +
                       $"<p>You have a session for <strong>{(game == null ? "any game" : HtmlEncode(game.Name))}</strong> starting soon." +
                       (string.IsNullOrWhiteSpace(ev.Details)
                           ? $""
                           : $"<p><em>{HtmlEncode(ev.Details)}</em>") +
                       (participants.Count > 1
                           ? $"<p>Don't forget to join {HtmlEncode(GetOtherMemberNames(participants, member))} <strong>{HtmlEncode(friendlyTime)}</strong>!"
                           : $"<p>It starts <strong>{HtmlEncode(friendlyTime)}!</strong></p>") +
                       $"<p>Have fun!";
            }
        }

        public async Task SendEventReminderAsync(long eventId)
        {
            var utcThreshold = DateTime.UtcNow - TimeSpan.FromHours(6);
            
            var groupEvent = await _db.GroupEvents
                .Include(x => x.Slots.Where(s => s.ProposedDateAndTimeUtc > utcThreshold).OrderBy(s => s.ProposedDateAndTimeUtc)).ThenInclude(x => x.Votes)
                .Include(x => x.Group.Memberships).ThenInclude(x => x.User)
                .Include(x => x.CantPlays)
                .Include(x => x.Game)
                .FirstOrDefaultAsync(x => x.Id == eventId);
            
            groupEvent.ReminderSentAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            if (!groupEvent.Slots.Any()) return;

            var missingVotes = _groupService.GetMissingVotes(groupEvent.Group.Memberships, groupEvent);
            var groupUrl = GetGroupUrl(groupEvent.Group);

            await EmailMembersAsync(
                missingVotes,
                $"Don't forget to vote on this {groupEvent.Game?.Name ?? "gaming"} session in {groupEvent.Group.Name}!",
                GetEmailMessage,
                x => x.UnsubscribeVoteReminder);

            string GetEmailMessage(Membership member)
            {
                return $"<p>Hi {HtmlEncode(member.DisplayName)}!" +
                       $"<p>A session for <strong>{(groupEvent.Game == null ? "any game" : HtmlEncode(groupEvent.Game.Name))}</strong> is waiting for your vote." +
                       $"<p>The proposed time slots are:" +
                       $"<ul>" +
                       string.Join("", groupEvent.Slots.Select(x => $"<li>{HtmlEncode(_dateService.FormatUtcToUserFriendlyDate(x.ProposedDateAndTimeUtc, member.User))}")) +
                       $"</ul>" +
                       $"<p><a href=\"{groupUrl}\">Go to your group's page to vote on it!</a>";
            }
        }
        
        
        private async Task EmailMembersAsync(
            IEnumerable<Membership> members, 
            string subject, 
            Func<Membership, string> getMessage, 
            Func<AppUser, bool> isUnsubscribed)
        {
            foreach (var member in members)
            {
                if (isUnsubscribed(member.User))
                    continue;

                await _emailSender.SendEmailAsync(
                    member.User.Email,
                    subject,
                    getMessage(member));
            }
        }

        private static string HtmlEncode(string value) => HtmlEncoder.Default.Encode(value);
        
        private string GetGroupUrl(Group group)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Page(
                pageName: "/Groups/Group",
                pageHandler: null,
                values: new {slug = group.Slug},
                protocol: _httpContextAccessor.HttpContext?.Request.Scheme ?? "http"
            );
        }
        
        private string GetOtherMemberNames(IEnumerable<Membership> members, Membership except)
        {
            var otherNames = members
                .Where(m => m != except)
                .Select(m => m.DisplayName)
                .ToList();
                
            var sb = new StringBuilder();
                
            for (var i = 0; i < otherNames.Count; i++)
            {
                var name = otherNames[i];

                if (i > 0) // is not first?
                {
                    if (i == otherNames.Count - 1) // is last? 
                        sb.Append(" and ");
                    else
                        sb.Append(", ");
                }

                sb.Append(name);
            }

            return sb.ToString();
        }
    }
}