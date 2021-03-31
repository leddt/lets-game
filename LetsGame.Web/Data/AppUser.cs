﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace LetsGame.Web.Data
{
    public class AppUser : IdentityUser
    {
        public ICollection<Group> Groups { get; set; }
        public ICollection<Membership> Memberships { get; set; }
        public ICollection<UserPushSubscription> PushSubscriptions { get; set; }
        
        public bool UnsubscribeNewEvent { get; set; }
        public bool UnsubscribeVoteReminder { get; set; }
        public bool UnsubscribeEventReminder { get; set; }
        
        public bool UnsubscribeNewEventPush { get; set; }
        public bool UnsubscribeVoteReminderPush { get; set; }
        public bool UnsubscribeEventReminderPush { get; set; }
    }
}