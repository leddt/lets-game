using LetsGame.Web.Services.Igdb.Models;

namespace LetsGame.Web.GraphQL.Types
{
    public class GameSearchResult : IdGraphType<Game>
    {
        private readonly Game _game;

        public GameSearchResult(Game game)
        {
            _game = game;
        }
        
        protected override object GetId() => _game.Id;
        public string Name => _game.Name;
        public string? IgdbImageId => _game.MainImage?.ImageId;
    }
}