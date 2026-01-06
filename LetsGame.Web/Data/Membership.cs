using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using NodaTime;

namespace LetsGame.Web.Data
{
    public class Membership
    {
        public AppUser? User { get; set; }
        public Group? Group { get; set; }

        [ForeignKey("User")] public string UserId { get; set; } = null!;
        [ForeignKey("Group")] public long GroupId { get; set; }
        
        public required string DisplayName { get; set; }
        
        public GroupRole Role { get; set; }
        
        public Instant? AvailableUntil { get; set; }
        public Instant? AvailabilityNotificationSentAt { get; set; }

        [MemberNotNullWhen(true, nameof(AvailableUntil))]
        public bool IsAvailableNow() => AvailableUntil > SystemClock.Instance.GetCurrentInstant();
    }
}