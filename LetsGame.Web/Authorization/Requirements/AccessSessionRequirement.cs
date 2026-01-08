using System;
using System.Linq;
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
    public class AccessSessionRequirement(bool manage) : IAuthorizationRequirement
    {
        public bool Manage { get; } = manage;
    }
    
    public class AccessSessionRequirementHandler(
        UserManager<AppUser> userManager,
        IDbContextFactory<ApplicationDbContext> dbFactory
    )
        : AuthorizationHandler<AccessSessionRequirement>
    {
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AccessSessionRequirement requirement)
        {
            bool isAuthorized;

            if (TryGetIdToAuthorize(context, out var id))
                isAuthorized = await AuthorizeForSessionId(context.User, requirement.Manage, id);
            else
                throw new InvalidOperationException("Can't resolve session to authorize");
            
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
                if (resolver.TryGetArgumentValue<string>("sessionId", out var sessionId))
                {
                    result = ID.ToLong<GroupEvent>(sessionId);
                    return true;
                }
                
                if (resolver.TryGetArgumentValue<ISessionIdInput>("input", out var input))
                {
                    result = ID.ToLong<GroupEvent>(input.SessionId);
                    return true;
                }

                if (resolver.TryGetParent<SessionGraphType>(out var sessionGraphType))
                {
                    result = ID.ToLong<GroupEvent>(sessionGraphType.Id);
                    return true;
                }

                if (resolver.TryGetArgumentValue<string>("id", out var id))
                {
                    result = ID.ToLong<GroupEvent>(id);
                    return true;
                }
            }

            result = 0;
            return false;
        }

        private async Task<bool> AuthorizeForSessionId(ClaimsPrincipal user, bool manage, long sessionId)
        {
            var userId = userManager.GetUserId(user);

            await using var db = await dbFactory.CreateDbContextAsync();

            return await db.GroupEvents
                .Where(x => x.Id == sessionId)
                .AnyAsync(e => e.CreatorId == userId ||
                               e.Group!.Memberships!.Any(m => m.UserId == userId && (!manage || m.Role == GroupRole.Owner)));
        }
    }
}