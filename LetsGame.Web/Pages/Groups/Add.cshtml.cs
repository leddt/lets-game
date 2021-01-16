using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LetsGame.Web.Pages.Groups
{
    public class AddModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly GroupService _groupService;
        private readonly UserManager<AppUser> _userManager;

        public AddModel(ApplicationDbContext db, GroupService groupService, UserManager<AppUser> userManager)
        {
            _db = db;
            _groupService = groupService;
            _userManager = userManager;
        }

        [BindProperty, Required, Display(Name = "Name", Prompt = "My amazing group")]
        public string GroupName { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var currentUserId = _userManager.GetUserId(User);
            var group = await _groupService.CreateGroupAsync(GroupName, currentUserId);

            return RedirectToPage("/Groups/Group", new {slug = group.Slug});
        }
    }
}