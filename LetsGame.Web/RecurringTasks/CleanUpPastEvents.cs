using System;
using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace LetsGame.Web.RecurringTasks
{
    public class CleanUpPastEvents : IRecurringTask
    {
        private readonly ApplicationDbContext _db;

        public CleanUpPastEvents(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task Run()
        {
            var threshold = SystemClock.Instance.GetCurrentInstant() - Duration.FromDays(1);
            
            var events = await _db.GroupEvents
                .Where(x => x.ChosenTime < threshold ||
                            x.Slots.All(s => s.ProposedTime < threshold))
                .ToListAsync();

            Console.WriteLine("{0} events to delete", events.Count);

            _db.GroupEvents.RemoveRange(events);
            await _db.SaveChangesAsync();
        }
    }
}