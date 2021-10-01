using System;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using LetsGame.Web.Authorization;
using LetsGame.Web.Data;
using LetsGame.Web.Extensions;
using LetsGame.Web.GraphQL.Types;
using LetsGame.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.GraphQL
{
    public class Mutation
    {
        public async Task<SessionSlotPayload> VoteSlot(
            [GraphQLType(typeof(IdType))] string slotId, 
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db,
            [Service] IAuthorizationService authorizationService,
            ClaimsPrincipal user)
        {
            var id = ID.ToLong<GroupEventSlot>(slotId);
            var slot = await db.GroupEventSlots.Include(x=> x.Event).FirstAsync(x => x.Id == id);

            await authorizationService.EnsureAuthorized(user, slot.Event.GroupId, AuthPolicies.ReadGroup);

            if (slot.Event.ChosenDateAndTimeUtc.HasValue)
                throw new Exception("Can't vote on confirmed event");

            await groupService.AddSlotVoteAsync(id);
            return new SessionSlotPayload(new SessionSlotGraphType(slot));
        }

        public async Task<SessionSlotPayload> UnvoteSlot(
            [GraphQLType(typeof(IdType))] string slotId, 
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db,
            [Service] IAuthorizationService authorizationService,
            ClaimsPrincipal user)
        {
            var id = ID.ToLong<GroupEventSlot>(slotId);
            var slot = await db.GroupEventSlots.Include(x => x.Event).FirstAsync(x => x.Id == id);

            await authorizationService.EnsureAuthorized(user, slot.Event.GroupId, AuthPolicies.ReadGroup);

            if (slot.Event.ChosenDateAndTimeUtc.HasValue)
                throw new Exception("Can't vote on confirmed event");
            
            await groupService.RemoveSlotVoteAsync(ID.ToLong<GroupEventSlot>(slotId));
            return new SessionSlotPayload(new SessionSlotGraphType(slot));
        }

        public async Task<ProposedSessionPayload> CantPlay(
            [GraphQLType(typeof(IdType))] string sessionId, 
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db,
            [Service] IAuthorizationService authorizationService,
            ClaimsPrincipal user)
        {
            var id = ID.ToLong<GroupEvent>(sessionId);
            var groupEvent = await db.GroupEvents.FindAsync(id);

            await authorizationService.EnsureAuthorized(user, groupEvent.GroupId, AuthPolicies.ReadGroup);

            if (groupEvent.ChosenDateAndTimeUtc.HasValue)
                throw new Exception("Can't vote on confirmed event");
            
            await groupService.SetCantPlayAsync(id);
            return new ProposedSessionPayload(new ProposedSessionGraphType(groupEvent));
        }

    }
}