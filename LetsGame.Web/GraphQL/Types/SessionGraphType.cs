using System.Threading.Tasks;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using LetsGame.Web.Data;

namespace LetsGame.Web.GraphQL.Types
{
    [InterfaceType]
    public abstract class SessionGraphType : IdGraphType<GroupEvent> 
    {
        protected readonly GroupEvent GroupEvent;

        public SessionGraphType(GroupEvent groupEvent)
        {
            GroupEvent = groupEvent;
        }

        protected override object GetId() => GroupEvent.Id;
        public string Details => GroupEvent.Details;

        public async Task<GroupGraphType> GetGroup(IResolverContext context)
        {
            var result = await context.LoadGroup(GroupEvent.GroupId);
            return new GroupGraphType(result);
        }
        
        public async Task<GameGraphType> GetGame(IResolverContext context)
        {
            if (GroupEvent.GameId == null) return null;
            var result = await context.LoadGame(GroupEvent.GameId.Value);
            return new GameGraphType(result);
        }

        public async Task<MembershipGraphType> Creator(IResolverContext context)
        {
            var result = await context.LoadMembership(GroupEvent.GroupId, GroupEvent.CreatorId);
            return new MembershipGraphType(result);
        }
    }
}