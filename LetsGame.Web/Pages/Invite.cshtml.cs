using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.Pages
{
    public class InviteModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;

        public InviteModel(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public GroupInvite Invite { get; set; }
        
        [BindProperty, Required, Display(Name = "Your display name", Prompt = "Your nickname in this group")]
        public string DisplayName { get; set; }

        public async Task<IActionResult> OnGet(
            [FromServices] ApplicationDbContext db, 
            string id)
        {
            await LoadInvite(db, id);
            
            if (Invite != null && User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                var membership = await db.Memberships.FirstOrDefaultAsync(x => x.UserId == userId && x.GroupId == Invite.GroupId);

                if (membership != null)
                    return RedirectToPage("/Groups/Group", new {slug = Invite.Group.Slug});
            }

            return Page();
        }

        public async Task<IActionResult> OnPost(
            [FromServices] ApplicationDbContext db, 
            [FromServices] GroupService groupService,
            string id)
        {
            if (!ModelState.IsValid)
            {
                await LoadInvite(db, id);
                return Page();
            }
            
            var userId = _userManager.GetUserId(User);
            var group = await groupService.AcceptInviteAsync(userId, DisplayName, id);
            
            return RedirectToPage("/Groups/Group", new {slug = group.Slug});
        }

        private async Task LoadInvite(ApplicationDbContext db, string id)
        {
            Invite = await db.GroupInvites
                .Include(x => x.Group)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}