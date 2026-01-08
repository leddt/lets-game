using System.Linq;
using System.Threading.Tasks;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using LetsGame.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Duration = NodaTime.Duration;

namespace LetsGame.Web.Controllers
{
    [ApiController]
    public class IcalController(ApplicationDbContext db) : ControllerBase
    {
        [HttpGet("group/{slug}.ics")]
        public async Task<IActionResult> GetGroupCalendarAsIcalAsync(string slug, string k)
        {
            var group = await db.Groups
                .Include(g => g.Events!.Where(e => e.ChosenTime.HasValue)).ThenInclude(e => e.Game)
                .Where(g => g.Slug == slug)
                .Where(g => g.SharingKey == k)
                .FirstOrDefaultAsync();

            if (group == null) return NotFound();

            var calendar = new Calendar
            {
                Properties = { new CalendarProperty("X-WR-CALNAME", group.Name)}
            };
            calendar.Events.AddRange(group.Events!.Select(GetCalendarEvent));

            var serialized = new CalendarSerializer().SerializeToString(calendar) ?? "";

            return Content(serialized, "text/calendar");
        }

        private CalendarEvent GetCalendarEvent(GroupEvent ev)
        {
            var start = ev.ChosenTime!.Value;
            var end = start + Duration.FromHours(1);
            
            return new CalendarEvent
            {
                Summary = ev.Game == null ? "Any game" : ev.Game.Name,
                Start = new CalDateTime(start.ToDateTimeUtc()),
                End = new CalDateTime(end.ToDateTimeUtc())
            };
        }
    }
}