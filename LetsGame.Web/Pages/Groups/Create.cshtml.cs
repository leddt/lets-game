using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LetsGame.Web.Pages.Groups
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly GroupService _groupService;

        public CreateModel(GroupService groupService)
        {
            _groupService = groupService;
        }

        [BindProperty, Required, Display(Name = "Group name", Prompt = "The public name for this group")]
        public string GroupName { get; set; }
        
        [BindProperty, Required, Display(Name = "Your display name", Prompt = "Your nickname in this group")]
        public string DisplayName { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            
            var group = await _groupService.CreateGroupAsync(GroupName, DisplayName);

            return RedirectToPage("/Groups/Group", new {slug = group.Slug});
        }
    }
}