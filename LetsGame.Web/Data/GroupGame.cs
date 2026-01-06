using System.ComponentModel.DataAnnotations.Schema;

namespace LetsGame.Web.Data
{
    public class GroupGame
    {
        public long Id { get; set; }
        
        public Group Group { get; set; }
        [ForeignKey("Group")] public long GroupId { get; set; }
        
        public long IgdbId { get; set; }
        public string Name { get; set; }
        public string? IgdbImageId { get; set; }
    }
}