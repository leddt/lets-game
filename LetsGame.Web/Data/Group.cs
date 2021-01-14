using System.Collections.Generic;

namespace LetsGame.Web.Data
{
    public class Group
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        
        public ICollection<AppUser> Members { get; set; }
        public ICollection<Membership> Memberships { get; set; }
        public ICollection<GroupGame> Games { get; set; }
    }
}