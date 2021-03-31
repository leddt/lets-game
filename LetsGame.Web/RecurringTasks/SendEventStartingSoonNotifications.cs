using System;
using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.RecurringTasks
{
    public class SendEventStartingSoonNotifications : IRecurringTask
    {
        private readonly ApplicationDbContext _db;
        private readonly INotificationService _notificationService;

        public SendEventStartingSoonNotifications(ApplicationDbContext db, INotificationService notificationService)
        {
            _db = db;
            _notificationService = notificationService;
        }

        public async Task Run()
        {
            var utcNow = DateTime.UtcNow;
            var utcThreshold = utcNow + TimeSpan.FromHours(2);

            var events = await _db.GroupEvents
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
                
                Console.WriteLine("Processing event {0} for group {1}", ev.Id, ev.GroupId);
                await _notificationService.NotifyEventStartingSoon(ev);
            }
        }
    }
}