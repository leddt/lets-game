using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace LetsGame.Web.Data
{
    public class GroupEventSlot
    {
        public long Id { get; set; }
        
        public GroupEvent Event { get; set; }
        [ForeignKey("Event")] public long EventId { get; set; }
        
        public Instant ProposedTime { get; set; }
        
        public ICollection<AppUser> Voters { get; set; }
        public ICollection<GroupEventSlotVote> Votes { get; set; }
    }
}