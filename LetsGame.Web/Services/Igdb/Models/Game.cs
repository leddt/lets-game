using System.Linq;

namespace LetsGame.Web.Services.Igdb.Models
{
    public class Game
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public Image[] Artworks { get; set; } = new Image[0];
        public Image[] Screenshots { get; set; } = new Image[0];

        public Image MainImage => Artworks?.FirstOrDefault() ?? Screenshots?.FirstOrDefault();
    }
}