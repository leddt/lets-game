using System;
using System.Collections.Generic;

namespace LetsGame.Web.Data
{
    public class GroupEventSlot
    {
        public long Id { get; set; }
        
        public GroupEvent Event { get; set; }
        
        public DateTime ProposedDateAndTimeUtc { get; set; }
        
        public ICollection<AppUser> Voters { get; set; }
        public ICollection<GroupEventSlotVote> Votes { get; set; }
    }
}