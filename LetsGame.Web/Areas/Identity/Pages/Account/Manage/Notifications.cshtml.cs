using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LetsGame.Web.Areas.Identity.Pages.Account.Manage
{
    public class Notifications : PageModel
    {
        [BindProperty, Display(Name = "A new event is added to one of my groups")] 
        public bool NewEvent { get; set; }
        
        [BindProperty, Display(Name = "An event I'm participating in is starting soon")] 
        public bool EventReminder { get; set; }
        
        [BindProperty, Display(Name = "An organizer reminds group members to vote on their event")] 
        public bool VoteReminder { get; set; }
        
        [TempData]
        public string StatusMessage { get; set; }

        public async Task OnGetAsync([FromServices] UserManager<AppUser> userManager)
        {
            var user = await userManager.FindByIdAsync(userManager.GetUserId(User));

            NewEvent = !user.UnsubscribeNewEvent;
            EventReminder = !user.UnsubscribeEventReminder;
            VoteReminder = !user.UnsubscribeVoteReminder;
        }

        public async Task<IActionResult> OnPostAsync([FromServices] UserManager<AppUser> userManager)
        {
            var user = await userManager.FindByIdAsync(userManager.GetUserId(User));

            user.UnsubscribeNewEvent = !NewEvent;
            user.UnsubscribeEventReminder = !EventReminder;
            user.UnsubscribeVoteReminder = !VoteReminder;

            await userManager.UpdateAsync(user);

            StatusMessage = "Preferences updated";
            
            return RedirectToPage();
        }
    }
}