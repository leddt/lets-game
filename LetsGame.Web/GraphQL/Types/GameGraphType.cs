using LetsGame.Web.Data;

namespace LetsGame.Web.GraphQL.Types
{
    public class GameGraphType : IdGraphType<GroupGame>
    {
        private readonly GroupGame _game;

        public GameGraphType(GroupGame game)
        {
            _game = game;
        }

        protected override object GetId() => _game.Id;
        public string Name => _game.Name;
        public string IgdbImageId => _game.IgdbImageId;
    }
}