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
    public class AccessGroupRequirement : IAuthorizationRequirement
    {
        public bool AsOwner { get; }

        public AccessGroupRequirement(bool asOwner)
        {
            AsOwner = asOwner;
        }
    }
    
    public class AccessGroupRequirementHandler : AuthorizationHandler<AccessGroupRequirement>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public AccessGroupRequirementHandler(
            UserManager<AppUser> userManager,
            IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _userManager = userManager;
            _dbFactory = dbFactory;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            AccessGroupRequirement requirement)
        {
            bool isAuthorized;

            if (TryGetIdToAuthorize(context, out var id))
                isAuthorized = await AuthorizeForGroupId(context.User, requirement.AsOwner, id);
            else if (TryGetSlugToAuthorize(context, out var slug))
                isAuthorized = await AuthorizeForGroupSlug(context.User, requirement.AsOwner, slug);
            else
                throw new InvalidOperationException("Can't resolve group to authorize");

            if (isAuthorized) 
                context.Succeed(requirement);
        }

        private static bool TryGetIdToAuthorize(AuthorizationHandlerContext context, out long result)
        {
            if (context.Resource is long longId)
            {
                result = longId;
                return true;
            }
            
            if (context.Resource is IResolverContext resolver)
            {
                if (resolver.TryGetArgumentValue<string>("groupId", out var groupId))
                {
                    result = ID.ToLong<Group>(groupId);
                    return true;
                }

                if (resolver.TryGetParent<GroupGraphType>(out var groupGraphType))
                {
                    result = ID.ToLong<Group>(groupGraphType.Id);
                    return true;
                }

                if (resolver.TryGetArgumentValue<string>("id", out var id))
                {
                    result = ID.ToLong<Group>(id);
                    return true;
                }
            }

            result = 0;
            return false;
        }

        private static bool TryGetSlugToAuthorize(AuthorizationHandlerContext context, out string result)
        {
            if (context.Resource is IResolverContext resolver)
            {
                if (resolver.TryGetArgumentValue<string>("slug", out var slug))
                {
                    result = slug;
                    return true;
                }
            }

            result = null;
            return false;
        }

        private async Task<bool> AuthorizeForGroupId(ClaimsPrincipal user, bool asOwner, long groupId)
        {
            var userId = _userManager.GetUserId(user);

            await using var db = _dbFactory.CreateDbContext();

            return await db.Memberships.AnyAsync(x => x.UserId == userId &&
                                                      x.GroupId == groupId &&
                                                      (asOwner == false || x.Role == GroupRole.Owner));
        }
        
        private async Task<bool> AuthorizeForGroupSlug(ClaimsPrincipal user, bool asOwner, string slug)
        {
            var userId = _userManager.GetUserId(user);

            await using var db = _dbFactory.CreateDbContext();

            return await db.Memberships.AnyAsync(x => x.UserId == userId &&
                                                      x.Group.Slug == slug &&
                                                      (asOwner == false || x.Role == GroupRole.Owner));
        }
    }
}