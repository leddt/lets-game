using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace LetsGame.Web.Data
{
    public class AppUser : IdentityUser
    {
        public ICollection<Group> Groups { get; set; }
        public ICollection<Membership> Memberships { get; set; }
    }
}