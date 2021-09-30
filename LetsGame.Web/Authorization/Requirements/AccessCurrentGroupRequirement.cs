using System;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using LetsGame.Web.Data;
using LetsGame.Web.GraphQL;
using LetsGame.Web.GraphQL.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.Authorization.Requirements
{
    public class AccessCurrentGroupRequirement : IAuthorizationRequirement
    {
        public bool AsOwner { get; }

        public AccessCurrentGroupRequirement(bool asOwner)
        {
            AsOwner = asOwner;
        }
    }
    
    public class AccessCurrentGroupRequirementHandler : AuthorizationHandler<AccessCurrentGroupRequirement>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public AccessCurrentGroupRequirementHandler(
            UserManager<AppUser> userManager,
            IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _userManager = userManager;
            _dbFactory = dbFactory;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            AccessCurrentGroupRequirement requirement)
        {
            var idToAuthorize = GetIdToAuthorize(context);
            var isAuthorized = await AuthorizeForGroupId(context.User, requirement.AsOwner, idToAuthorize);
            
            if (isAuthorized) context.Succeed(requirement);
        }

        private static long GetIdToAuthorize(AuthorizationHandlerContext context)
        {
            if (context.Resource is IResolverContext resolver)
            {
                if (resolver.TryGetArgumentValue<string>("groupId", out var groupId))
                    return ID.ToLong<Group>(groupId);

                if (resolver.TryGetParent<GroupGraphType>(out var groupGraphType))
                    return ID.ToLong<Group>(groupGraphType.Id);
                
                if (resolver.TryGetArgumentValue<string>("id", out var id))
                    return ID.ToLong<Group>(id);
            }

            throw new InvalidOperationException("Can't resolve ID to authorize");
        }

        private async Task<bool> AuthorizeForGroupId(ClaimsPrincipal user, bool asOwner, long groupId)
        {
            var userId = _userManager.GetUserId(user);

            await using var db = _dbFactory.CreateDbContext();

            return await db.Memberships.AnyAsync(x => x.UserId == userId &&
                                                      x.GroupId == groupId &&
                                                      (asOwner == false || x.Role == GroupRole.Owner));
        }
    }
}