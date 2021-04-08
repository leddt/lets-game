using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace LetsGame.Web.Data
{
    public class Membership
    {
        public AppUser User { get; set; }
        public Group Group { get; set; }
        
        [ForeignKey("User")] public string UserId { get; set; }
        [ForeignKey("Group")] public long GroupId { get; set; }
        
        public string DisplayName { get; set; }
        
        public GroupRole Role { get; set; }
        
        public DateTime? AvailableUntilUtc { get; set; }
        public DateTime? AvailabilityNotificationSentAtUtc { get; set; }

        [MemberNotNullWhen(true, nameof(AvailableUntilUtc))]
        public bool IsAvailableNow() => AvailableUntilUtc > DateTime.UtcNow;
    }
}