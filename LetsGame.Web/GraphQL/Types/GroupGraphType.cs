using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate;
using LetsGame.Web.Authorization;
using LetsGame.Web.Data;
using LetsGame.Web.GraphQL.DataLoaders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using NodaTime;

namespace LetsGame.Web.GraphQL.Types
{
    public class GroupGraphType(Group group) : IdGraphType<Group>
    {
        protected override object GetId() => group.Id;

        public string Name => group.Name;
        public string Slug => group.Slug;
        public string SharingKey => group.SharingKey;

        public async Task<IEnumerable<MembershipGraphType>> GetMembers(IMembershipsByGroupIdDataLoader loader)
        {
            var members = await loader.LoadAsync(group.Id) ?? [];
            return members.Select(x => new MembershipGraphType(x));
        }
        
        public async Task<MembershipGraphType?> GetSelf(
            IMembershipsByGroupIdDataLoader loader, 
            ClaimsPrincipal user, 
            [Service] UserManager<AppUser> userManager)
        {
            var userId = userManager.GetUserId(user);
            var members = await loader.LoadAsync(group.Id) ?? [];
            var self = members.FirstOrDefault(x => x.UserId == userId);
            return self == null ? null : new MembershipGraphType(self);
        }

        public async Task<IEnumerable<GameGraphType>> GetGames(IGroupGamesByGroupIdDataLoader loader)
        {
            var games = await loader.LoadAsync(group.Id) ?? [];
            return games.Select(x => new GameGraphType(x));
        }

        public async Task<IEnumerable<ProposedSessionGraphType>> GetProposedSessions(IGroupEventWithSlotsByGroupIdDataLoader loader)
        {
            var events = await loader.LoadAsync(group.Id) ?? [];
            var now = SystemClock.Instance.GetCurrentInstant();
            
            return events
                .Where(x => x.ChosenTime == null)
                .Where(x => x.Slots!.Any(s => s.ProposedTime > now))
                .OrderBy(x => x.Slots!.Where(s => s.ProposedTime > now).Min(s => s.ProposedTime))
                .Select(x => new ProposedSessionGraphType(x));
        }

        public async Task<IEnumerable<UpcomingSessionGraphType>> GetUpcomingSessions(IGroupEventWithSlotsByGroupIdDataLoader loader)
        {
            var events = await loader.LoadAsync(group.Id) ?? [];
            var cutoff = SystemClock.Instance.GetCurrentInstant() - Duration.FromHours(6);
            
            return events
                .Where(x => x.ChosenTime != null)
                .Where(x => x.ChosenTime > cutoff)
                .OrderBy(x => x.ChosenTime)
                .Select(x => new UpcomingSessionGraphType(x));
        }

        public async Task<IEnumerable<InviteGraphType>?> GetInvites(
            IGroupInvitesByGroupIdDataLoader loader, 
            ClaimsPrincipal user,
            [Service] IAuthorizationService authorizationService)
        {
            var auth = await authorizationService.AuthorizeAsync(user, group.Id, AuthPolicies.ManageGroup);
            if (!auth.Succeeded) return null;
            
            var invites = await loader.LoadAsync(group.Id) ?? [];
            return invites.Select(x => new InviteGraphType(x));
        }
    }
}