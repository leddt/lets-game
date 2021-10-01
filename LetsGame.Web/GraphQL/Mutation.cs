using System;
using System.Linq;
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
        [Authorize(Policy = AuthPolicies.ReadSlot)]
        public async Task<SessionSlotPayload> VoteSlot(
            [GraphQLType(typeof(IdType))] string slotId, 
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db)
        {
            var id = ID.ToLong<GroupEventSlot>(slotId);
            
            var slot = await db.GroupEventSlots.Include(x=> x.Event).FirstAsync(x => x.Id == id);
            if (slot.Event.ChosenDateAndTimeUtc.HasValue)
                throw new Exception("Can't vote on confirmed event");

            await groupService.AddSlotVoteAsync(id);
            return new SessionSlotPayload(new SessionSlotGraphType(slot));
        }

        [Authorize(Policy = AuthPolicies.ReadSlot)]
        public async Task<SessionSlotPayload> UnvoteSlot(
            [GraphQLType(typeof(IdType))] string slotId, 
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db)
        {
            var id = ID.ToLong<GroupEventSlot>(slotId);
            
            var slot = await db.GroupEventSlots.Include(x => x.Event).FirstAsync(x => x.Id == id);
            if (slot.Event.ChosenDateAndTimeUtc.HasValue)
                throw new Exception("Can't vote on confirmed event");
            
            await groupService.RemoveSlotVoteAsync(ID.ToLong<GroupEventSlot>(slotId));
            return new SessionSlotPayload(new SessionSlotGraphType(slot));
        }

        [Authorize(Policy = AuthPolicies.ReadSession)]
        public async Task<ProposedSessionPayload> CantPlay(
            [GraphQLType(typeof(IdType))] string sessionId, 
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db)
        {
            var id = ID.ToLong<GroupEvent>(sessionId);

            var groupEvent = await db.GroupEvents.FindAsync(id);
            if (groupEvent.ChosenDateAndTimeUtc.HasValue)
                throw new Exception("Can't vote on confirmed event");
            
            await groupService.SetCantPlayAsync(id);
            return new ProposedSessionPayload(new ProposedSessionGraphType(groupEvent));
        }

        [Authorize(Policy = AuthPolicies.ManageSession)]
        public async Task<ProposedSessionPayload> SendReminder(
            [GraphQLType(typeof(IdType))] string sessionId,
            [Service] INotificationService notificationService,
            [Service] ApplicationDbContext db)
        {
            var id = ID.ToLong<GroupEvent>(sessionId);

            var groupEvent = await db.GroupEvents.FindAsync(id);
            if (groupEvent.ChosenDateAndTimeUtc.HasValue)
                throw new Exception("Can't send reminder on confirmed event");
            
            await notificationService.SendEventReminderAsync(id);
            return new ProposedSessionPayload(new ProposedSessionGraphType(groupEvent));
        }

        [Authorize(Policy = AuthPolicies.ManageSlot)]
        public async Task<GroupPayload> SelectWinningSlot(
            [GraphQLType(typeof(IdType))] string slotId,
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db)
        {
            var id = ID.ToLong<GroupEventSlot>(slotId);

            var groupEvent = await db.GroupEvents
                .Include(x => x.Group)
                .Where(x => x.Slots.Any(s => s.Id == id))
                .FirstOrDefaultAsync();
            
            if (groupEvent.ChosenDateAndTimeUtc.HasValue)
                throw new Exception("Can't pick slot on confirmed event");
            
            await groupService.PickSlotAsync(ID.ToLong<GroupEventSlot>(slotId));
            
            return new GroupPayload(new GroupGraphType(groupEvent.Group));
        }
    }
}