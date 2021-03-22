using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LetsGame.Web.Data
{
    public class GroupEvent
    {
        public long Id { get; set; }
        
        [ForeignKey("Game")] public long? GameId { get; set; }
        [ForeignKey("Group")] public long GroupId { get; set; }
        [ForeignKey("Creator")] public string CreatorId { get; set; }
        
        public Group Group { get; set; }
        public GroupGame Game { get; set; }
        public AppUser Creator { get; set; }
        public ICollection<GroupEventSlot> Slots { get; set; }
        public ICollection<GroupEventCantPlay> CantPlays { get; set; }
        
        public DateTime? ChosenDateAndTimeUtc { get; set; }
        public DateTime? StartingSoonNotificationSentAtUtc { get; set; }
        public DateTime? ReminderSentAtUtc { get; set; }
        
        public string Details { get; set; }
    }
}