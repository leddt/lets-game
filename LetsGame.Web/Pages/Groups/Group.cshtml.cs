using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.Services;
using LetsGame.Web.Services.Itad.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.Pages.Groups
{
    public class GroupModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public GroupModel(ApplicationDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public Group Group { get; set; }
        public IEnumerable<GroupEvent> ProposedEvents => Group.Events.Where(x => x.ChosenDateAndTimeUtc == null);
        public IEnumerable<GroupEvent> UpcomingEvent => Group.Events.Where(x => x.ChosenDateAndTimeUtc.HasValue);
        
        [BindProperty]
        public int SlotId { get; set; }
        
        public ItadSearchResult[] SearchResults { get; set; }
        
        public async Task<IActionResult> OnGetAsync(string slug)
        {
            var utcNow = DateTime.UtcNow;
            
            Group = await _db.Groups
                .TagWith("Load group page data")
                .AsSplitQuery()
                .Include(x => x.Games)
                .Include(x => x.Events
                    .Where(e => e.ChosenDateAndTimeUtc > utcNow || 
                                e.ChosenDateAndTimeUtc == null && e.Slots.Any(s => s.ProposedDateAndTimeUtc > utcNow)))
                    .ThenInclude(x => x.Game)
                .Include(x => x.Events)
                    .ThenInclude(x => x.Slots.OrderBy(s => s.ProposedDateAndTimeUtc))
                    .ThenInclude(x => x.Votes)
                    .ThenInclude(x => x.Voter)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync(x => x.Slug == slug);

            if (Group == null) return NotFound();
            
            return Page();
        }

        public async Task<IActionResult> OnPostDelete(string slug)
        {
            var currentUserId = _userManager.GetUserId(User);
            var group = await _db.Groups.FirstOrDefaultAsync(x =>
                x.Slug == slug &&
                x.Memberships.Any(m => m.Role == GroupRole.Owner && 
                                       m.User.Id == currentUserId));

            if (group != null)
            {
                _db.Groups.Remove(group);
                await _db.SaveChangesAsync();
            }

            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostVoteSlot(
            [FromServices] GroupService groupService,
            string slug)
        {
            var userId = _userManager.GetUserId(User);
            
            await groupService.AddSlotVote(SlotId, userId);

            return RedirectToPage("Group", new {slug});
        }
    }
}