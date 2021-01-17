using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LetsGame.Web.Pages.Groups
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly GroupService _groupService;
        private readonly UserManager<AppUser> _userManager;

        public CreateModel(ApplicationDbContext db, GroupService groupService, UserManager<AppUser> userManager)
        {
            _db = db;
            _groupService = groupService;
            _userManager = userManager;
        }

        [BindProperty, Required, Display(Name = "Group name", Prompt = "The public name for this group")]
        public string GroupName { get; set; }
        
        [BindProperty, Required, Display(Name = "Your display name", Prompt = "Your nickname in this group")]
        public string DisplayName { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            
            var currentUserId = _userManager.GetUserId(User);
            var group = await _groupService.CreateGroupAsync(GroupName, currentUserId, DisplayName);

            return RedirectToPage("/Groups/Group", new {slug = group.Slug});
        }
    }
}