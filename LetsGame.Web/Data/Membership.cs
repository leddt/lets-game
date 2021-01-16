using System.ComponentModel.DataAnnotations.Schema;

namespace LetsGame.Web.Data
{
    public class Membership
    {
        public AppUser User { get; set; }
        public Group Group { get; set; }
        
        [ForeignKey("User")] public string UserId { get; set; }
        [ForeignKey("Group")] public long GroupId { get; set; }
        
        public GroupRole Role { get; set; }
    }
}