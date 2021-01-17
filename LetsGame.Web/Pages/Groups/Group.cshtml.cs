using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.Services;
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
        public Membership Membership { get; set; }
        public IEnumerable<GroupEvent> ProposedEvents => Group.Events.Where(x => x.ChosenDateAndTimeUtc == null);
        public IEnumerable<GroupEvent> UpcomingEvents => Group.Events.Where(x => x.ChosenDateAndTimeUtc.HasValue).OrderBy(x => x.ChosenDateAndTimeUtc);
        
        [BindProperty]
        public int SlotId { get; set; }
        
        public string GetDisplayName(string userId) =>
            Group.Memberships.FirstOrDefault(x => x.UserId == userId)?.DisplayName ?? "Unknown member";

        public bool IsGroupOwner => Membership.Role == GroupRole.Owner;
        
        public async Task<IActionResult> OnGetAsync(string slug)
        {
            var utcNow = DateTime.UtcNow;
            var currentUserId = _userManager.GetUserId(User);
            
            Group = await _db.Groups
                .TagWith("Load group page data")
                .AsSplitQuery()
                .Include(x => x.Memberships)
                .Include(x => x.Games)
                .Include(x => x.Events
                    .Where(e => e.ChosenDateAndTimeUtc > utcNow || 
                                e.ChosenDateAndTimeUtc == null && e.Slots.Any(s => s.ProposedDateAndTimeUtc > utcNow)))
                    .ThenInclude(x => x.Game)
                .Include(x => x.Events)
                    .ThenInclude(x => x.Slots.OrderBy(s => s.ProposedDateAndTimeUtc))
                    .ThenInclude(x => x.Votes)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync(x => x.Slug == slug);
            
            Membership = Group.Memberships.FirstOrDefault(x => x.UserId == currentUserId);

            if (Group == null) return NotFound();
            if (Membership == null) return NotFound();
            
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

        public async Task<IActionResult> OnPostPickSlot(
            [FromServices] GroupService groupService,
            string slug)
        {
            await groupService.PickSlot(SlotId);
            
            return RedirectToPage("Group", new {slug});
        }
    }
}