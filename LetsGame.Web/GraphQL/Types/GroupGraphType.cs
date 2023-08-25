using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Resolvers;
using LetsGame.Web.Authorization;
using LetsGame.Web.Data;
using LetsGame.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using NodaTime;

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
        public string SharingKey => _group.SharingKey;

        public async Task<IEnumerable<MembershipGraphType>> GetMembers(IResolverContext context)
        {
            var members = await context.LoadMembershipsByGroupId(_group.Id);
            return members.Select(x => new MembershipGraphType(x));
        }
        
        public async Task<MembershipGraphType> GetSelf(
            IResolverContext context, 
            ClaimsPrincipal user, 
            [Service] UserManager<AppUser> userManager)
        {
            var userId = userManager.GetUserId(user);
            var members = await context.LoadMembershipsByGroupId(_group.Id);
            var self = members.FirstOrDefault(x => x.UserId == userId);
            return self == null ? null : new MembershipGraphType(self);
        }

        public async Task<IEnumerable<GameGraphType>> GetGames(IResolverContext context)
        {
            var games = await context.LoadGamesByGroupId(_group.Id);
            return games.Select(x => new GameGraphType(x));
        }

        public async Task<IEnumerable<ProposedSessionGraphType>> GetProposedSessions(IResolverContext context)
        {
            var events = await context.LoadEventsWithSlotsByGroupId(_group.Id);
            var now = SystemClock.Instance.GetCurrentInstant();
            
            return events
                .Where(x => x.ChosenTime == null)
                .Where(x => x.Slots.Any(s => s.ProposedTime > now))
                .OrderBy(x => x.Slots.Where(s => s.ProposedTime > now).Min(s => s.ProposedTime))
                .Select(x => new ProposedSessionGraphType(x));
        }

        public async Task<IEnumerable<UpcomingSessionGraphType>> GetUpcomingSessions(IResolverContext context)
        {
            var events = await context.LoadEventsWithSlotsByGroupId(_group.Id);
            var cutoff = SystemClock.Instance.GetCurrentInstant() - Duration.FromHours(6);
            
            return events
                .Where(x => x.ChosenTime != null)
                .Where(x => x.ChosenTime > cutoff)
                .OrderBy(x => x.ChosenTime)
                .Select(x => new UpcomingSessionGraphType(x));
        }

        public async Task<IEnumerable<InviteGraphType>> GetInvites(
            IResolverContext context, 
            ClaimsPrincipal user,
            [Service] IAuthorizationService authorizationService)
        {
            var auth = await authorizationService.AuthorizeAsync(user, _group.Id, AuthPolicies.ManageGroup);
            if (!auth.Succeeded) return null;
            
            var invites = await context.LoadInvitesByGroupId(_group.Id);
            return invites.Select(x => new InviteGraphType(x));
        }
    }
}