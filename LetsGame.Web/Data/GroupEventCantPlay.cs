using System;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace LetsGame.Web.Data
{
    public class GroupEventCantPlay
    {
        public GroupEvent Event { get; set; }
        public AppUser User { get; set; }
        
        public Instant AddedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();
        
        [ForeignKey("Event")] public long EventId { get; set; }
        [ForeignKey("User")] public string UserId { get; set; }
    }
}