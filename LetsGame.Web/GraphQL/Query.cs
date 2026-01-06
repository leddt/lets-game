using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using LetsGame.Web.Authorization;
using LetsGame.Web.Data;
using LetsGame.Web.GraphQL.DataLoaders;
using LetsGame.Web.GraphQL.Types;
using LetsGame.Web.Services.Igdb;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace LetsGame.Web.GraphQL
{
    public class Query
    {
        public UserGraphType GetMe(ClaimsPrincipal user)
        {
            return new UserGraphType(user);
        }
        
        [Authorize]
        public async Task<IEnumerable<GroupGraphType>> GetGroups(
            ClaimsPrincipal user,
            [Service] UserManager<AppUser> userManager,
            ApplicationDbContext db)
        {
            var userId = userManager.GetUserId(user);
            
            var groups = await db.Memberships
                .Where(x => x.UserId == userId)
                .Select(x => x.Group!)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return groups.Select(x => new GroupGraphType(x));
        }

        [Authorize(Policy = AuthPolicies.ReadGroup)]
        public async Task<GroupGraphType?> GetGroup(IGroupByIdDataLoader loader, [GraphQLType(typeof(IdType))] string id)
        {
            var groupId = ID.ToLong<Group>(id);

            var group = await loader.LoadAsync(groupId);

            return group == null 
                ? null 
                : new GroupGraphType(group);
        }

        [Authorize(Policy = AuthPolicies.ReadGroup)]
        public async Task<GroupGraphType?> GetGroupBySlug(IGroupBySlugDataLoader loader, string slug)
        {
            var group = await loader.LoadAsync(slug);

            return group == null 
                ? null 
                : new GroupGraphType(group);
        }

        [Authorize]
        public async Task<IEnumerable<UpcomingSessionGraphType>> GetUpcomingSessions(
            ClaimsPrincipal user,
            [Service] UserManager<AppUser> userManager,
            ApplicationDbContext db)
        {
            var userId = userManager.GetUserId(user);
            var threshold = SystemClock.Instance.GetCurrentInstant() - Duration.FromHours(3);

            var sessions = await db.GroupEvents
                .Include(x => x.Slots)
                .Where(x => x.Group.Memberships.Any(m => m.UserId == userId))
                .Where(x => x.ChosenTime > threshold)
                .OrderBy(x => x.ChosenTime)
                .ToListAsync();

            return sessions.Select(x => new UpcomingSessionGraphType(x));
        }

        [Authorize]
        public async Task<IEnumerable<ProposedSessionGraphType>> GetProposedSessions(
            ClaimsPrincipal user,
            [Service] UserManager<AppUser> userManager,
            ApplicationDbContext db)
        {
            var userId = userManager.GetUserId(user);
            var now = SystemClock.Instance.GetCurrentInstant();

            var sessions = await db.GroupEvents
                .Include(x => x.Slots)
                .Where(x => x.Group.Memberships.Any(m => m.UserId == userId))
                .Where(x => x.ChosenTime == null)
                .Where(x => x.Slots.Any(s => s.ProposedTime > now))
                .OrderBy(x => x.Slots.Where(s => s.ProposedTime > now).Min(s => s.ProposedTime))
                .ToListAsync();

            return sessions.Select(x => new ProposedSessionGraphType(x));
        }

        [Authorize]
        public async Task<IEnumerable<GameSearchResult>> SearchGames(string term, [Service] IGameSearcher searcher)
        {
            var results = await searcher.SearchGamesAsync(term);

            return results.Select(x => new GameSearchResult(x));
        }
    }
}