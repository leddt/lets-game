using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using LetsGame.Web.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.Controllers
{
    [ApiController]
    public class IcalController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public IcalController(ApplicationDbContext db, IDataProtectionProvider dataProtectionProvider)
        {
            _db = db;
            _dataProtectionProvider = dataProtectionProvider;
        }

        [HttpGet("group/{slug}.ics")]
        public async Task<IActionResult> GetGroupCalendarAsIcalAsync(string slug, string u, string t)
        {
            var protector = _dataProtectionProvider.CreateProtector("ical auth");
            var expected = $"{slug}:{u}";
            var actual = protector.Unprotect(t);
            if (expected != actual) return NotFound();
            
            var group = await _db.Groups
                .Include(g => g.Events.Where(e => e.ChosenDateAndTimeUtc.HasValue)).ThenInclude(e => e.Game)
                .Where(g => g.Slug == slug)
                .FirstOrDefaultAsync();

            var calendar = new Calendar
            {
                Properties = { new CalendarProperty("X-WR-CALNAME", group.Name)}
            };
            calendar.Events.AddRange(group.Events.Select(GetCalendarEvent));

            var serialized = new CalendarSerializer().SerializeToString(calendar);

            return Content(serialized, "text/calendar");
        }

        private CalendarEvent GetCalendarEvent(GroupEvent ev)
        {
            var dt = DateTime.SpecifyKind(ev.ChosenDateAndTimeUtc.Value, DateTimeKind.Utc);

            return new CalendarEvent
            {
                Summary = ev.Game.Name,
                Start = new CalDateTime(dt),
                End = new CalDateTime(dt + TimeSpan.FromHours(1))
            };
        }
    }
}