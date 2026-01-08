using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace LetsGame.Web.Data
{
    public class GroupEventSlotVote
    {
        public GroupEventSlot? Slot { get; set; }
        public AppUser? Voter { get; set; }

        [ForeignKey("Voter")] public string VoterId { get; set; } = null!;
        [ForeignKey("Slot")] public long SlotId { get; set; }
        
        public Instant VotedAt { get; set; }
    }
}