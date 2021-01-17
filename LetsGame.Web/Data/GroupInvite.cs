using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LetsGame.Web.Data
{
    public class GroupInvite
    {
        public string Id { get; set; }
        
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [ForeignKey("Group")]
        public long GroupId { get; set; }
        public bool IsSingleUse { get; set; }

        public Group Group { get; set; }
    }
}