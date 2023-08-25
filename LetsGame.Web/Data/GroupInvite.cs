using System;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace LetsGame.Web.Data
{
    public class GroupInvite
    {
        public string Id { get; set; }
        
        public Instant CreatedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();

        [ForeignKey("Group")]
        public long GroupId { get; set; }
        public bool IsSingleUse { get; set; }

        public Group Group { get; set; }
    }
}