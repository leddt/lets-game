using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LetsGame.Web.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public string UserId { get; set; }
        public IList<Group> Groups { get; set; }
        public IList<GroupEvent> AllEvents { get; set; }

        public IEnumerable<GroupEvent> UpcomingEvents => AllEvents
            .Where(x => x.ChosenDateAndTimeUtc.HasValue)
            .OrderBy(x => x.ChosenDateAndTimeUtc);

        public IEnumerable<GroupEvent> ProposedEvents => AllEvents
            .Where(x => x.ChosenDateAndTimeUtc == null)
            .OrderBy(x => x.Slots.Min(s => s.ProposedDateAndTimeUtc));
        
        public IndexModel(ApplicationDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task OnGet()
        {
            UserId = _userManager.GetUserId(User);
            var now = DateTime.UtcNow;
            
            Groups = await _db.Groups
                .Where(x => x.Memberships.Any(m => m.UserId == UserId))
                .OrderBy(x => x.Name)
                .ToListAsync();

            AllEvents = await _db.GroupEvents
                .Include(x => x.Group)
                .Include(x => x.Game)
                .Include(x => x.Slots.Where(s => s.ProposedDateAndTimeUtc > now)).ThenInclude(x => x.Votes)
                .Where(x => x.Group.Memberships.Any(m => m.UserId == UserId))
                .Where(x => x.ChosenDateAndTimeUtc > now ||
                            x.ChosenDateAndTimeUtc == null && x.Slots.Any(s => s.ProposedDateAndTimeUtc > now))
                .ToListAsync();
        }

    }
}