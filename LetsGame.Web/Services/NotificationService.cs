using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.Services.Igdb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Routing;
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
        private readonly IEmailSender _emailSender;
        private readonly IPushSender _pushSender;
        private readonly LinkGenerator _linkGenerator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationService(
            ApplicationDbContext db, 
            DateService dateService,
            IEmailSender emailSender,
            IPushSender pushSender,
            LinkGenerator linkGenerator,
            IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _dateService = dateService;
            _emailSender = emailSender;
            _pushSender = pushSender;
            _linkGenerator = linkGenerator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task NotifyEventAdded(GroupEvent newEvent)
        {
            var allGroupMembers = await _db.Memberships
                .Include(x => x.User).ThenInclude(x => x.PushSubscriptions)
                .Where(x => x.GroupId == newEvent.GroupId)
                .ToListAsync();

            var creator = allGroupMembers.First(x => x.UserId == newEvent.CreatorId);
            var game = await _db.GroupGames.FindAsync(newEvent.GameId);
            var group = await _db.Groups.FindAsync(newEvent.GroupId);
            var slotsUtc = newEvent.Slots.Select(x => x.ProposedDateAndTimeUtc).ToList();
            var groupUrl = GetGroupUrl(group);

            var membersToNotify = allGroupMembers.Where(x => x.UserId != newEvent.CreatorId).ToList();
            
            await EmailMembersAsync(
                membersToNotify, 
                $"A new session has been proposed in {group.Name}", 
                GetEmailMessage,
                x => x.UnsubscribeNewEvent);

            await PushMembersAsync(
                membersToNotify,
                new SimpleNotificationPayload
                {
                    Title = group.Name,
                    Body = $"New session proposed for {game?.Name ?? "any game"}",
                    Image = Image.GetScreenshotMedUrl(game?.IgdbImageId),
                    Url = GetGroupUrl(group) 
                },
                x => x.UnsubscribeNewEventPush);

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
                .Include(x => x.User).ThenInclude(x => x.PushSubscriptions)
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

            await PushMembersAsync(
                participants,
                new SimpleNotificationPayload
                {
                    Title = group.Name,
                    Body = $"A session for {game?.Name ?? "any game"} is starting at {eventSlot.ProposedDateAndTimeUtc.TimeOfDay}",
                    Image = Image.GetScreenshotMedUrl(game?.IgdbImageId),
                    Url = GetGroupUrl(group)
                },
                x => x.UnsubscribeEventReminderPush);
            
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
                .Include(x => x.Group.Memberships).ThenInclude(x => x.User).ThenInclude(x => x.PushSubscriptions)
                .Include(x => x.CantPlays)
                .Include(x => x.Game)
                .FirstOrDefaultAsync(x => x.Id == eventId);
            
            groupEvent.ReminderSentAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            if (!groupEvent.Slots.Any()) return;

            var missingVotes = groupEvent.GetMissingVotes().ToList();
            var groupUrl = GetGroupUrl(groupEvent.Group);

            await EmailMembersAsync(
                missingVotes,
                $"Don't forget to vote on this {groupEvent.Game?.Name ?? "gaming"} session in {groupEvent.Group.Name}!",
                GetEmailMessage,
                x => x.UnsubscribeVoteReminder);

            await PushMembersAsync(
                missingVotes,
                new SimpleNotificationPayload
                {
                    Title = groupEvent.Group.Name,
                    Body = $"Remember to vote on the {groupEvent.Game?.Name ?? "gaming"} session",
                    Image = Image.GetScreenshotMedUrl(groupEvent.Game?.IgdbImageId),
                    Url = GetGroupUrl(groupEvent.Group)
                },
                x => x.UnsubscribeVoteReminderPush);

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

        private async Task PushMembersAsync(
            IEnumerable<Membership> members, 
            SimpleNotificationPayload payload,
            Func<AppUser, bool> isUnsubscribed)
        {
            var allSubscriptions = new List<string>();
            
            foreach (var member in members)
            {
                if (isUnsubscribed(member.User))
                    continue;

                allSubscriptions.AddRange(member.User.PushSubscriptions.Select(x => x.SubscriptionJson));
            }

            await _pushSender.SendNotification(allSubscriptions, payload);
        }

        private static string HtmlEncode(string value) => HtmlEncoder.Default.Encode(value);
        
        private string GetGroupUrl(Group group)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                return _linkGenerator.GetPathByPage(
                    "/Groups/Group",
                    null,
                    new {slug = group.Slug});
            }
            else
            {
                return _linkGenerator.GetUriByPage(
                    _httpContextAccessor.HttpContext,
                    "/Groups/Group",
                    null,
                    new {slug = group.Slug});
                
                // var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                //
                // return urlHelper.Page(
                //     pageName: "/Groups/Group",
                //     pageHandler: null,
                //     values: new {slug = group.Slug},
                //     protocol: _httpContextAccessor.HttpContext?.Request.Scheme ?? "http"
                // );
            }
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