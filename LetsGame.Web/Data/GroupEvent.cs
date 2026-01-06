using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using NodaTime;

namespace LetsGame.Web.Data
{
    public class GroupEvent
    {
        public long Id { get; set; }
        
        [ForeignKey("Game")] public long? GameId { get; set; }
        [ForeignKey("Group")] public long GroupId { get; set; }
        [ForeignKey("Creator")] public string? CreatorId { get; set; }
        
        public Group Group { get; set; }
        public GroupGame? Game { get; set; }
        public AppUser? Creator { get; set; }
        public ICollection<GroupEventSlot> Slots { get; set; }
        public ICollection<GroupEventCantPlay> CantPlays { get; set; }
        
        public Instant? ChosenTime { get; set; }
        public Instant? AllVotesInNotificationSentAt { get; set; }
        public Instant? StartingSoonNotificationSentAt { get; set; }
        public Instant? ReminderSentAt { get; set; }
        
        public string Details { get; set; }
        

        public IEnumerable<Membership> GetMissingVotes()
        {
            if (Slots == null) throw new InvalidOperationException("Event Slots must be loaded");
            if (Slots.Any(s => s.Votes == null)) throw new InvalidOperationException("Slot Votes must be loaded");
            if (CantPlays == null) throw new InvalidOperationException("Event CantPlays must be loaded");
            if (Group == null) throw new InvalidOperationException("Group must be loaded");
            if (Group.Memberships == null) throw new InvalidOperationException("Group memberships must be loaded");
            
            var voterIds = Slots.SelectMany(s => s.Votes).Select(v => v.VoterId);
            var cantPlays = CantPlays.Select(x => x.UserId);
            var allVoterIds = voterIds.Union(cantPlays).Distinct();
            
            return Group.Memberships.Where(x => !allVoterIds.Contains(x.UserId));
        }

        public GroupEventSlot GetChosenSlot()
        {
            return Slots.FirstOrDefault(s => s.ProposedTime == ChosenTime);
        }
    }
}