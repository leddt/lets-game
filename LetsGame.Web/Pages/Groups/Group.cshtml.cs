using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.Data;
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
        
        [BindProperty]
        public string SearchText { get; set; }
        
        public ItadSearchResult[] SearchResults { get; set; }
        
        public async Task<IActionResult> OnGetAsync(string slug)
        {
            Group = await _db.Groups
                .Include(x => x.Games)
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
    }
}