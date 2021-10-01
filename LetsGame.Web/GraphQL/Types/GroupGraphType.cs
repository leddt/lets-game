using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using LetsGame.Web.Authorization;
using LetsGame.Web.Data;

namespace LetsGame.Web.GraphQL.Types
{
    public class GroupGraphType : IdGraphType<Group>
    {
        private readonly Group _group;

        public GroupGraphType(Group group)
        {
            _group = group;
        }

        protected override object GetId() => _group.Id;

        public string Name => _group.Name;
        public string Slug => _group.Slug;

        public async Task<IEnumerable<MembershipGraphType>> GetMembers(IResolverContext context)
        {
            var members = await context.LoadMembershipsByGroupId(_group.Id);
            return members.Select(x => new MembershipGraphType(x));
        }

        public async Task<IEnumerable<GameGraphType>> GetGames(IResolverContext context)
        {
            var games = await context.LoadGamesByGroupId(_group.Id);
            return games.Select(x => new GameGraphType(x));
        }

        public async Task<IEnumerable<ProposedSessionGraphType>> GetProposedSessions(IResolverContext context)
        {
            var events = await context.LoadEventsWithSlotsByGroupId(_group.Id);
            
            return events
                .Where(x => x.ChosenDateAndTimeUtc == null)
                .Select(x => new ProposedSessionGraphType(x));
        }

        public async Task<IEnumerable<UpcomingSessionGraphType>> GetUpcomingSessions(IResolverContext context)
        {
            var events = await context.LoadEventsWithSlotsByGroupId(_group.Id);
            
            return events
                .Where(x => x.ChosenDateAndTimeUtc != null)
                .Select(x => new UpcomingSessionGraphType(x));
        }

        [Authorize(Policy = AuthPolicies.ManageGroup)]
        public async Task<IEnumerable<InviteGraphType>> GetInvites(IResolverContext context)
        {
            var invites = await context.LoadInvitesByGroupId(_group.Id);
            return invites.Select(x => new InviteGraphType(x));
        }
    }
}