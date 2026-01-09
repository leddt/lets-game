using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using LetsGame.Web.Data;
using LetsGame.Web.GraphQL;
using LetsGame.Web.GraphQL.Types;
using LetsGame.Web.Services.EventSystem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.Authorization.Requirements
{
    public class AccessGroupRequirement(bool asOwner) : IAuthorizationRequirement
    {
        public bool AsOwner { get; } = asOwner;
    }
    
    public class AccessGroupRequirementHandler(
        UserManager<AppUser> userManager,
        IDbContextFactory<ApplicationDbContext> dbFactory,
        IEventSystem eventSystem
    ) : AuthorizationHandler<AccessGroupRequirement>
    {
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            AccessGroupRequirement requirement)
        {
            eventSystem.Enrich(x => x.AddData("AuthGroupAsOwner", requirement.AsOwner));
            
            bool isAuthorized;

            if (TryGetIdToAuthorize(context, out var id))
            {
                eventSystem.Enrich(x => x.AddData("AuthGroupId", id));
                isAuthorized = await AuthorizeForGroupId(context.User, requirement.AsOwner, id);
            }
            else if (TryGetSlugToAuthorize(context, out var slug))
            {
                eventSystem.Enrich(x => x.AddData("AuthGroupSlug", slug));
                isAuthorized = await AuthorizeForGroupSlug(context.User, requirement.AsOwner, slug);
            }
            else
                throw new InvalidOperationException("Can't resolve group to authorize");

            eventSystem.Enrich(x => x.AddData("AuthGroupSucceeded", isAuthorized));
            
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

        private static bool TryGetSlugToAuthorize(AuthorizationHandlerContext context, [NotNullWhen(true)] out string? result)
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
            var userId = userManager.GetUserId(user);

            await using var db = await dbFactory.CreateDbContextAsync();

            return await db.Memberships.AnyAsync(x => x.UserId == userId &&
                                                      x.GroupId == groupId &&
                                                      (asOwner == false || x.Role == GroupRole.Owner));
        }
        
        private async Task<bool> AuthorizeForGroupSlug(ClaimsPrincipal user, bool asOwner, string slug)
        {
            var userId = userManager.GetUserId(user);

            await using var db = await dbFactory.CreateDbContextAsync();

            return await db.Memberships.AnyAsync(x => x.UserId == userId &&
                                                      x.Group!.Slug == slug &&
                                                      (asOwner == false || x.Role == GroupRole.Owner));
        }
    }
}