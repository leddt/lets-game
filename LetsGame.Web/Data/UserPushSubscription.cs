using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LetsGame.Web.Data
{
    public class UserPushSubscription
    {
        public long Id { get; set; }
        
        [Required, ForeignKey("UserId")]
        public AppUser? User { get; set; }
        public string UserId { get; set; } = null!;
        
        public required string SubscriptionJson { get; set; }
    }
}