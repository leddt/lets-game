namespace LetsGame.Web.Data
{
    public class Membership
    {
        public AppUser User { get; set; }
        public Group Group { get; set; }
        
        public GroupRole Role { get; set; }
    }
}