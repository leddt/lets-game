using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.RecurringTasks
{
    public class SendEventStartingSoonNotifications : IRecurringTask
    {
        private readonly ApplicationDbContext _db;
        private readonly BatchMemberMailer _batchMemberMailer;
        private readonly DateService _dateService;

        public SendEventStartingSoonNotifications(ApplicationDbContext db, BatchMemberMailer batchMemberMailer, DateService dateService)
        {
            _db = db;
            _batchMemberMailer = batchMemberMailer;
            _dateService = dateService;
        }

        public async Task Run()
        {
            var utcNow = DateTime.UtcNow;
            var utcThreshold = utcNow + TimeSpan.FromHours(2);

            var events = await _db.GroupEvents
                .AsSplitQuery()
                .Include(x => x.Game)
                .Include(x => x.Group).ThenInclude(x => x.Memberships).ThenInclude(x => x.User)
                .Include(x => x.Slots).ThenInclude(x => x.Votes)
                .Where(x => x.ChosenDateAndTimeUtc > utcNow)
                .Where(x => x.ChosenDateAndTimeUtc < utcThreshold)
                .Where(x => x.StartingSoonNotificationSentAtUtc == null)
                .ToListAsync();
            
            Console.WriteLine("{0} events to notify", events.Count);

            foreach (var ev in events)
            {
                // Mark as sent BEFORE sending the emails. If it crashes we don't want to be sending duplicate emails.
                ev.StartingSoonNotificationSentAtUtc = utcNow;
                await _db.SaveChangesAsync();
                
                await SendEventNotifications(ev);
            }
        }

        public async Task SendEventNotifications(GroupEvent ev)
        {
            Console.WriteLine("Processing event {0} for group {1}", ev.Id, ev.GroupId);

            // Sanity checks
            if (ev.ChosenDateAndTimeUtc == null) return;
            
            var eventSlot = ev.Slots.FirstOrDefault(x => x.ProposedDateAndTimeUtc == ev.ChosenDateAndTimeUtc);
            if (eventSlot == null) return;
            
            var participants = ev.Group.Memberships
                .Where(m => eventSlot.Votes.Select(v => v.VoterId).Contains(m.UserId))
                .OrderBy(m => m.DisplayName)
                .ToList();

            Console.WriteLine("Notifying {0} participants", participants.Count);
            await _batchMemberMailer.EmailMembersAsync(
                participants,
                $"A session is starting soon in {ev.Group.Name}",
                x => GetMessage(ev, participants, x),
                x => x.UnsubscribeEventReminder);
        }

        private static string Encode(string value) => HtmlEncoder.Default.Encode(value);
            
        private string GetMessage(GroupEvent ev, ICollection<Membership> members, Membership member)
        {
            var friendlyTime = _dateService.FormatUtcToUserFriendlyDate(ev.ChosenDateAndTimeUtc.Value, member.User);

            return $"<p>Hi {Encode(member.DisplayName)}!" +
                   $"<p>You have a session for <strong>{(ev.Game == null ? "any game" : Encode(ev.Game.Name))}</strong> starting soon." +
                   (string.IsNullOrWhiteSpace(ev.Details)
                       ? $""
                       : $"<p><em>{Encode(ev.Details)}</em>") +
                   (members.Count > 1
                       ? $"<p>Don't forget to join {Encode(GetOtherMemberNames(members, member))} <strong>{Encode(friendlyTime)}</strong>!"
                       : $"<p>It starts <strong>{Encode(friendlyTime)}!</strong></p>") +
                   $"<p>Have fun!";
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