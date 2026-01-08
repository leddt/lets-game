using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LetsGame.Web.Areas.Identity.Pages.Account.Manage
{
    public class Notifications : PageModel
    {
        public string[]? PushSubscriptions { get; set; }
        public string? VapidPublicKey { get; set; }
        
        [BindProperty] 
        public string? AddPushSubscription { get; set; }
        [BindProperty] 
        public string? RemovePushSubscription { get; set; }
        
        [BindProperty] 
        public bool NewEvent { get; set; }
        [BindProperty] 
        public bool NewEventPush { get; set; }
        
        [BindProperty] 
        public bool EventReminder { get; set; }
        [BindProperty] 
        public bool EventReminderPush { get; set; }
        
        [BindProperty] 
        public bool VoteReminder { get; set; }
        [BindProperty] 
        public bool VoteReminderPush { get; set; }
        
        [BindProperty] 
        public bool MemberAvailable { get; set; }
        [BindProperty] 
        public bool MemberAvailablePush { get; set; }
        
        [BindProperty] 
        public bool SlotPicked { get; set; }
        [BindProperty] 
        public bool SlotPickedPush { get; set; }
        
        [BindProperty] 
        public bool AllVotesIn { get; set; }
        [BindProperty] 
        public bool AllVotesInPush { get; set; }
        
        [TempData]
        public string? StatusMessage { get; set; }

        public async Task OnGetAsync([FromServices] UserManager<AppUser> userManager, [FromServices] ApplicationDbContext db, [FromServices] IConfiguration config)
        {
            var user = await db.Users
                .Include(x => x.PushSubscriptions)
                .FirstAsync(x => x.Id == userManager.GetUserId(User));
            
            PushSubscriptions = user.PushSubscriptions!.Select(x => x.SubscriptionJson).ToArray();
            VapidPublicKey = config["vapid:publicKey"];
            
            NewEvent = !user.UnsubscribeNewEvent;
            EventReminder = !user.UnsubscribeEventReminder;
            VoteReminder = !user.UnsubscribeVoteReminder;
            MemberAvailable = !user.UnsubscribeMemberAvailable;
            
            NewEventPush = !user.UnsubscribeNewEventPush;
            EventReminderPush = !user.UnsubscribeEventReminderPush;
            VoteReminderPush = !user.UnsubscribeVoteReminderPush;
            MemberAvailablePush = !user.UnsubscribeMemberAvailablePush;

            SlotPicked = !user.UnsubscribeSlotPicked;
            SlotPickedPush = !user.UnsubscribeSlotPickedPush;
            AllVotesIn = !user.UnsubscribeAllVotesIn;
            AllVotesInPush = !user.UnsubscribeAllVotesInPush;
        }

        public async Task<IActionResult> OnPostAsync([FromServices] UserManager<AppUser> userManager, [FromServices] ApplicationDbContext db)
        {
            var user = await db.Users
                .Include(x => x.PushSubscriptions)
                .FirstAsync(x => x.Id == userManager.GetUserId(User));

            user.UnsubscribeNewEvent = !NewEvent;
            user.UnsubscribeEventReminder = !EventReminder;
            user.UnsubscribeVoteReminder = !VoteReminder;
            user.UnsubscribeMemberAvailable = !MemberAvailable;
            
            user.UnsubscribeNewEventPush = !NewEventPush;
            user.UnsubscribeEventReminderPush = !EventReminderPush;
            user.UnsubscribeVoteReminderPush = !VoteReminderPush;
            user.UnsubscribeMemberAvailablePush = !MemberAvailablePush;

            user.UnsubscribeSlotPicked = !SlotPicked;
            user.UnsubscribeSlotPickedPush = !SlotPickedPush;
            user.UnsubscribeAllVotesIn = !AllVotesIn;
            user.UnsubscribeAllVotesInPush = !AllVotesInPush;

            if (!string.IsNullOrWhiteSpace(AddPushSubscription))
            {
                user.PushSubscriptions!.Add(new UserPushSubscription {SubscriptionJson = AddPushSubscription});
            }

            if (!string.IsNullOrWhiteSpace(RemovePushSubscription))
            {
                var sub = user.PushSubscriptions!.FirstOrDefault(x => x.SubscriptionJson == RemovePushSubscription);
                if (sub != null) user.PushSubscriptions!.Remove(sub);
            }

            await db.SaveChangesAsync();

            StatusMessage = "Preferences updated";
            
            return RedirectToPage();
        }
    }
}