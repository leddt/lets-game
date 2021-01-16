using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LetsGame.Web.Data
{
    public class GroupEventSlotVote
    {
        public GroupEventSlot Slot { get; set; }
        public AppUser Voter { get; set; }
        
        [ForeignKey("Voter")] public string VoterId { get; set; }
        [ForeignKey("Slot")] public long SlotId { get; set; }
        
        public DateTime VotedAtUtc { get; set; }
    }
}