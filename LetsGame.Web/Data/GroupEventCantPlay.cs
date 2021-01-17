using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LetsGame.Web.Data
{
    public class GroupEventCantPlay
    {
        public GroupEvent Event { get; set; }
        public AppUser User { get; set; }
        
        public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;
        
        [ForeignKey("Event")] public long EventId { get; set; }
        [ForeignKey("User")] public string UserId { get; set; }
    }
}