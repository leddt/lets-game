using System.Threading.Tasks;
using GreenDonut;
using HotChocolate.Types;
using LetsGame.Web.Data;
using LetsGame.Web.GraphQL.DataLoaders;

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

        public async Task<GroupGraphType> GetGroup(IGroupByIdDataLoader loader)
        {
            var result = await loader.LoadRequiredAsync(GroupEvent.GroupId);
            return new GroupGraphType(result);
        }
        
        public async Task<GameGraphType?> GetGame(IGroupGameByIdDataLoader loader)
        {
            if (GroupEvent.GameId == null) return null;
            var result = await loader.LoadRequiredAsync(GroupEvent.GameId.Value);
            return new GameGraphType(result);
        }

        public async Task<MembershipGraphType?> Creator(IMembershipByGroupIdAndUserIdDataLoader loader)
        {
            if (GroupEvent.CreatorId == null) return null;
            var result = await loader.LoadRequiredAsync((GroupEvent.GroupId, GroupEvent.CreatorId));
            return new MembershipGraphType(result);
        }
    }
}