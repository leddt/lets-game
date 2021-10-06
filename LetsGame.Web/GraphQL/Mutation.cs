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
using LetsGame.Web.Services.Igdb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using NodaTime;

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

        public async Task<UpcomingSessionPayload> JoinSession(
            [GraphQLType(typeof(IdType))] string sessionId, 
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db)
        {
            var id = ID.ToLong<GroupEvent>(sessionId);

            var slot = await db.GroupEventSlots
                .Include(x => x.Event)
                .Where(x => x.EventId == id)
                .Where(x => x.ProposedDateAndTimeUtc == x.Event.ChosenDateAndTimeUtc)
                .FirstOrDefaultAsync();

            if (slot == null) throw new Exception("Can't find chosen slot");

            await groupService.AddSlotVoteAsync(slot.Id);
            return new UpcomingSessionPayload(new UpcomingSessionGraphType(slot.Event));
        }

        public async Task<UpcomingSessionPayload> LeaveSession(
            [GraphQLType(typeof(IdType))] string sessionId, 
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db)
        {
            var id = ID.ToLong<GroupEvent>(sessionId);

            var slot = await db.GroupEventSlots
                .Include(x => x.Event)
                .Where(x => x.EventId == id)
                .Where(x => x.ProposedDateAndTimeUtc == x.Event.ChosenDateAndTimeUtc)
                .FirstOrDefaultAsync();

            if (slot == null) throw new Exception("Can't find chosen slot");

            await groupService.RemoveSlotVoteAsync(slot.Id);
            return new UpcomingSessionPayload(new UpcomingSessionGraphType(slot.Event));
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

        [Authorize(Policy = AuthPolicies.ManageGroup)]
        public async Task<GroupPayload> CreateInvite(
            [GraphQLType(typeof(IdType))] string groupId,
            bool singleUse,
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db)
        {
            var id = ID.ToLong<Group>(groupId);
            
            await groupService.CreateInviteAsync(id, singleUse);

            var group = await db.Groups.FindAsync(id);
            return new GroupPayload(new GroupGraphType(group));
        }

        [Authorize(Policy = AuthPolicies.ManageGroup)]
        public async Task<GroupPayload> DeleteInvite(
            [GraphQLType(typeof(IdType))] string groupId,
            string inviteCode,
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db)
        {
            var id = ID.ToLong<Group>(groupId);
            
            var invite = await db.GroupInvites
                .Include(x => x.Group)
                .Where(x => x.Id == inviteCode)
                .Where(x => x.GroupId == id)
                .FirstOrDefaultAsync();
            if (invite == null) throw new Exception("Invalid invite");

            await groupService.DeleteInviteAsync(inviteCode);

            return new GroupPayload(new GroupGraphType(invite.Group));
        }

        [Authorize(Policy = AuthPolicies.ReadGroup)]
        public async Task<GroupPayload> SetAvailable(
            [GraphQLType(typeof(IdType))] string groupId,
            int lengthInSeconds,
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db)
        {
            var id = ID.ToLong<Group>(groupId);
            await groupService.SetAvailableFor(id, lengthInSeconds);

            var group = await db.Groups.FindAsync(id);
            return new GroupPayload(new GroupGraphType(group));
        }
        
        [Authorize(Policy = AuthPolicies.ReadGroup)]
        public async Task<GroupPayload> SetUnavailable(
            [GraphQLType(typeof(IdType))] string groupId,
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db)
        {
            var id = ID.ToLong<Group>(groupId);
            await groupService.SetAvailableFor(id, -1);

            var group = await db.Groups.FindAsync(id);
            return new GroupPayload(new GroupGraphType(group));
        }

        [Authorize(Policy = AuthPolicies.ManageGroup)]
        public async Task<GroupPayload> AddGame(
            [GraphQLType(typeof(IdType))] string groupId,
            [GraphQLType(typeof(IdType))] string gameId,
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db)
        {
            await groupService.AddGameToGroupAsync(ID.ToLong<Group>(groupId), ID.ToLong<Game>(gameId));

            var group = await db.Groups.FindAsync(ID.ToLong<Group>(groupId));
            return new GroupPayload(new GroupGraphType(group));
        }
        

        [Authorize(Policy = AuthPolicies.ManageGroup)]
        public async Task<GroupPayload> RemoveGame(
            [GraphQLType(typeof(IdType))] string groupId,
            [GraphQLType(typeof(IdType))] string gameId,
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db)
        {
            await groupService.RemoveGameFromGroupAsync(ID.ToLong<Group>(groupId), ID.ToLong<GroupGame>(gameId));

            var group = await db.Groups.FindAsync(ID.ToLong<Group>(groupId));
            return new GroupPayload(new GroupGraphType(group));
        }


        [Authorize(Policy = AuthPolicies.ReadGroup)]
        public async Task<GroupPayload> ProposeSession(
            [GraphQLType(typeof(IdType))] string groupId,
            [GraphQLType(typeof(IdType))] string gameId,
            LocalDateTime[] dateTimes,
            string details,
            [Service] GroupService groupService,
            [Service] DateService dateService,
            [Service] ApplicationDbContext db)
        {
            var utcDateTimes = dateTimes
                .Select(x => dateService.ConvertFromUserLocalTimeToUtc(x))
                .ToArray();
            
            await groupService.ProposeEventAsync(
                ID.ToLong<Group>(groupId),
                ID.ToLong<GroupGame>(gameId),
                details,
                utcDateTimes);
            
            var group = await db.Groups.FindAsync(ID.ToLong<Group>(groupId));
            return new GroupPayload(new GroupGraphType(group));
        }
    }
}