using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using LetsGame.Web.Authorization;
using LetsGame.Web.Data;
using LetsGame.Web.Extensions;
using LetsGame.Web.GraphQL.Types;
using LetsGame.Web.Services.Igdb;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.GraphQL
{
    public class Query
    {
        public UserGraphType GetMe(ClaimsPrincipal user)
        {
            return new UserGraphType(user);
        }
        
        [Authorize, UseDbContext(typeof(ApplicationDbContext))]
        public async Task<IEnumerable<GroupGraphType>> GetGroups(
            ClaimsPrincipal user,
            [Service] UserManager<AppUser> userManager,
            [ScopedService] ApplicationDbContext db)
        {
            var userId = userManager.GetUserId(user);
            
            var groups = await db.Memberships
                .Where(x => x.UserId == userId)
                .Select(x => x.Group)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return groups.Select(x => new GroupGraphType(x));
        }

        [Authorize(Policy = AuthPolicies.ReadGroup)]
        public async Task<GroupGraphType> GetGroup(IResolverContext context, [GraphQLType(typeof(IdType))] string id)
        {
            var groupId = ID.ToLong<Group>(id);

            var group = await context.LoadGroup(groupId);

            return group == null 
                ? null 
                : new GroupGraphType(group);
        }

        [Authorize(Policy = AuthPolicies.ReadGroup)]
        public async Task<GroupGraphType> GetGroupBySlug(IResolverContext context, string slug)
        {
            var group = await context.LoadGroupBySlug(slug);

            return group == null 
                ? null 
                : new GroupGraphType(group);
        }

        public async Task<IEnumerable<GameSearchResult>> SearchGames(string term, [Service] IGameSearcher searcher)
        {
            var results = await searcher.SearchGamesAsync(term);

            return results.Select(x => new GameSearchResult(x));
        }
    }
}