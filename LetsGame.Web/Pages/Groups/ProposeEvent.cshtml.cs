using System;
using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.Helpers;
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

        public ProposeEvent(ApplicationDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public Group Group { get; set; }
        public DateTime MinDateTime => DateHelpers.RemoveSeconds(DateHelpers.ConvertFromUtcToUserTimezone(DateTime.UtcNow));
        
        [BindProperty]
        public long? PickedGameId { get; set; }
        
        [BindProperty]
        public string Details { get; set; }

        [BindProperty] 
        public DateTime?[] ProposedDatesAndTimes { get; set; }
        
        
        public async Task OnGetAsync(string slug)
        {
            await LoadGroupAsync(slug);
        }

        public async Task OnPostPickGameAsync(string slug)
        {
            ProposedDatesAndTimes = new DateTime?[12];
            await LoadGroupAsync(slug);
        }

        public async Task<IActionResult> OnPostProposeAsync(string slug)
        {
            await LoadGroupAsync(slug);
            if (PickedGameId == null) return RedirectToPage("ProposeEvent", new {slug});

            var userId = _userManager.GetUserId(User);
            
            var groupEvent = new GroupEvent
            {
                Group = Group,
                GameId = PickedGameId.Value,
                Details = Details,
                CreatorId = userId,
                Slots = ProposedDatesAndTimes
                    .Where(x => x.HasValue)
                    .Select(dt => new GroupEventSlot
                    {
                        ProposedDateAndTimeUtc = DateHelpers.ConvertFromUserTimezoneToUtc(dt.Value)
                    })
                    .ToList()
            };

            _db.GroupEvents.Add(groupEvent);
            await _db.SaveChangesAsync();

            return RedirectToPage("Group", new {slug});
        }

        private async Task LoadGroupAsync(string slug)
        {
            Group = await _db.Groups
                .AsSplitQuery()
                .Include(x => x.Games)
                .FirstOrDefaultAsync(x => x.Slug == slug);
        }
    }
}