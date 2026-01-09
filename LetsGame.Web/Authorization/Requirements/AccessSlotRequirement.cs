using System;
using System.Linq;
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
    public class AccessSlotRequirement : IAuthorizationRequirement
    {
        public bool Manage { get; }

        public AccessSlotRequirement(bool manage)
        {
            Manage = manage;
        }
    }
    
    public class AccessSlotRequirementHandler(
        UserManager<AppUser> userManager,
        IDbContextFactory<ApplicationDbContext> dbFactory,
        IEventSystem eventSystem
    ) : AuthorizationHandler<AccessSlotRequirement>
    {
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AccessSlotRequirement requirement)
        {
            eventSystem.Enrich(x => x.AddData("AuthSlotManage", requirement.Manage));
            
            bool isAuthorized;

            if (TryGetIdToAuthorize(context, out var id))
            {
                eventSystem.Enrich(x => x.AddData("AuthSlotId", id));
                isAuthorized = await AuthorizeForSlotId(context.User, requirement.Manage, id);
            }
            else
                throw new InvalidOperationException("Can't resolve session to authorize");
            
            eventSystem.Enrich(x => x.AddData("AuthSlotSucceeded", isAuthorized));
            
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
                if (resolver.TryGetArgumentValue<string>("slotId", out var sessionId))
                {
                    result = ID.ToLong<GroupEventSlot>(sessionId);
                    return true;
                }

                if (resolver.TryGetParent<SessionSlotGraphType>(out var slotGraphType))
                {
                    result = ID.ToLong<GroupEventSlot>(slotGraphType.Id);
                    return true;
                }

                if (resolver.TryGetArgumentValue<string>("id", out var id))
                {
                    result = ID.ToLong<GroupEventSlot>(id);
                    return true;
                }
            }

            result = 0;
            return false;
        }

        private async Task<bool> AuthorizeForSlotId(ClaimsPrincipal user, bool manage, long slotId)
        {
            var userId = userManager.GetUserId(user);

            await using var db = dbFactory.CreateDbContext();

            return await db.GroupEventSlots
                .Where(x => x.Id == slotId)
                .AnyAsync(s => s.Event!.CreatorId == userId ||
                               s.Event.Group!.Memberships!.Any(m => m.UserId == userId && (!manage || m.Role == GroupRole.Owner)));
        }
    }
}