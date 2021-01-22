using System;
using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.Pages.Groups
{
    [Authorize]
    public class ProposeEvent : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly DateService _dateService;
        private readonly GroupService _groupService;

        public ProposeEvent(ApplicationDbContext db, UserManager<AppUser> userManager, DateService dateService, GroupService groupService)
        {
            _db = db;
            _userManager = userManager;
            _dateService = dateService;
            _groupService = groupService;
        }

        public Group Group { get; set; }
        public DateTime MinDateTime => _dateService.RemoveSeconds(_dateService.ConvertFromUtcToUserTimezone(DateTime.UtcNow));
        
        [BindProperty]
        public long? PickedGameId { get; set; }
        
        [BindProperty]
        public string Details { get; set; }

        [BindProperty] 
        public DateTime?[] ProposedDatesAndTimes { get; set; }
        
        
        public async Task<IActionResult> OnGetAsync(string slug)
        {
            await LoadGroupAsync(slug);
            if (Group == null) return NotFound();
            
            return Page();
        }

        public async Task<IActionResult> OnPostPickGameAsync(string slug)
        {
            ProposedDatesAndTimes = new DateTime?[12];
            await LoadGroupAsync(slug);
            if (Group == null) return NotFound();
            
            return Page();
        }

        public async Task<IActionResult> OnPostProposeAsync(string slug)
        {
            await LoadGroupAsync(slug);
            if (Group == null) return NotFound();
            if (PickedGameId == null) return RedirectToPage("ProposeEvent", new {slug});

            if (ProposedDatesAndTimes == null || ProposedDatesAndTimes.Length == 0)
            {
                ModelState.AddModelError("ProposedDatesAndTimes", "Please choose at least one time slot.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _groupService.ProposeEventAsync(
                groupId: Group.Id,
                gameId: PickedGameId.Value,
                details: Details,
                slotsUtc: ProposedDatesAndTimes
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .Select(x => _dateService.ConvertFromUserTimezoneToUtc(x))
                    .OrderBy(x => x)
                    .ToArray());

            return RedirectToPage("Group", new {slug});
        }

        private async Task LoadGroupAsync(string slug)
        {
            var userId = _userManager.GetUserId(User);
            Group = await _db.Groups
                .Include(x => x.Games)
                .Where(x => x.Memberships.Any(m => m.UserId == userId))
                .FirstOrDefaultAsync(x => x.Slug == slug);
        }
    }
}