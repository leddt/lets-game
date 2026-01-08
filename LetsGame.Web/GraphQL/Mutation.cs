using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using LetsGame.Web.Authorization;
using LetsGame.Web.Data;
using LetsGame.Web.Extensions;
using LetsGame.Web.GraphQL.Types;
using LetsGame.Web.Services;
using LetsGame.Web.Services.Igdb.Models;
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
            [Service] ApplicationDbContext db,
            [Service] ITopicEventSender sender)
        {
            var id = ID.ToLong<GroupEventSlot>(slotId);
            
            var slot = await db.GroupEventSlots.Include(x=> x.Event).FirstAsync(x => x.Id == id);
            if (slot.Event!.ChosenTime.HasValue)
                throw new Exception("Can't vote on confirmed event");

            await groupService.AddSlotVoteAsync(id);
            
            var session = new ProposedSessionGraphType(slot.Event);
            await sender.Send(session);
            return new SessionSlotPayload(new SessionSlotGraphType(slot));
        }

        [Authorize(Policy = AuthPolicies.ReadSlot)]
        public async Task<SessionSlotPayload> UnvoteSlot(
            [GraphQLType(typeof(IdType))] string slotId, 
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db,
            [Service] ITopicEventSender sender)
        {
            var id = ID.ToLong<GroupEventSlot>(slotId);
            
            var slot = await db.GroupEventSlots.Include(x => x.Event).FirstAsync(x => x.Id == id);
            if (slot.Event!.ChosenTime.HasValue)
                throw new Exception("Can't vote on confirmed event");
            
            await groupService.RemoveSlotVoteAsync(ID.ToLong<GroupEventSlot>(slotId));
            
            var session = new ProposedSessionGraphType(slot.Event);
            await sender.Send(session);
            return new SessionSlotPayload(new SessionSlotGraphType(slot));
        }

        [Authorize(Policy = AuthPolicies.ReadSession)]
        public async Task<UpcomingSessionPayload> JoinSession(
            [GraphQLType(typeof(IdType))] string sessionId, 
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db,
            [Service] ITopicEventSender sender)
        {
            var id = ID.ToLong<GroupEvent>(sessionId);

            var slot = await db.GroupEventSlots
                .Include(x => x.Event)
                .Where(x => x.EventId == id)
                .Where(x => x.ProposedTime == x.Event!.ChosenTime)
                .FirstOrDefaultAsync();

            if (slot == null) throw new Exception("Can't find chosen slot");

            await groupService.AddSlotVoteAsync(slot.Id);
            
            var result = new UpcomingSessionGraphType(slot.Event!);
            await sender.Send(result);
            return new UpcomingSessionPayload(result);
        }

        [Authorize(Policy = AuthPolicies.ReadSession)]
        public async Task<UpcomingSessionPayload> LeaveSession(
            [GraphQLType(typeof(IdType))] string sessionId, 
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db,
            [Service] ITopicEventSender sender)
        {
            var id = ID.ToLong<GroupEvent>(sessionId);

            var slot = await db.GroupEventSlots
                .Include(x => x.Event)
                .Where(x => x.EventId == id)
                .Where(x => x.ProposedTime == x.Event!.ChosenTime)
                .FirstOrDefaultAsync();

            if (slot == null) throw new Exception("Can't find chosen slot");

            await groupService.RemoveSlotVoteAsync(slot.Id);
            
            var result = new UpcomingSessionGraphType(slot.Event!);
            await sender.Send(result);
            return new UpcomingSessionPayload(result);
        }

        [Authorize(Policy = AuthPolicies.ReadSession)]
        public async Task<ProposedSessionPayload> CantPlay(
            [GraphQLType(typeof(IdType))] string sessionId, 
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db,
            [Service] ITopicEventSender sender)
        {
            var id = ID.ToLong<GroupEvent>(sessionId);

            var groupEvent = await db.GroupEvents.FindAsync(id);
            if (groupEvent == null) throw new Exception("Can't find event");
            if (groupEvent.ChosenTime.HasValue)
                throw new Exception("Can't vote on confirmed event");
            
            await groupService.SetCantPlayAsync(id);
            
            var result = new ProposedSessionGraphType(groupEvent);
            await sender.Send(result);
            return new ProposedSessionPayload(result);
        }

        [Authorize(Policy = AuthPolicies.ManageSession)]
        public async Task<ProposedSessionPayload> SendReminder(
            [GraphQLType(typeof(IdType))] string sessionId,
            [Service] INotificationService notificationService,
            [Service] ApplicationDbContext db,
            [Service] ITopicEventSender sender)
        {
            var id = ID.ToLong<GroupEvent>(sessionId);

            var groupEvent = await db.GroupEvents.FindAsync(id);
            if (groupEvent == null) throw new Exception("Can't find event");
            if (groupEvent.ChosenTime.HasValue)
                throw new Exception("Can't send reminder on confirmed event");
            
            await notificationService.SendEventReminderAsync(id);
            
            var result = new ProposedSessionGraphType(groupEvent);
            await sender.Send(result);
            return new ProposedSessionPayload(result);
        }

        [Authorize(Policy = AuthPolicies.ManageSlot)]
        public async Task<GroupPayload> SelectWinningSlot(
            [GraphQLType(typeof(IdType))] string slotId,
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db,
            [Service] ITopicEventSender sender)
        {
            var id = ID.ToLong<GroupEventSlot>(slotId);

            var groupEvent = await db.GroupEvents
                .Include(x => x.Group)
                .Where(x => x.Slots!.Any(s => s.Id == id))
                .FirstOrDefaultAsync();
            
            if (groupEvent == null) throw new Exception("Can't find event");
            if (groupEvent.ChosenTime.HasValue)
                throw new Exception("Can't pick slot on confirmed event");
            
            await groupService.PickSlotAsync(ID.ToLong<GroupEventSlot>(slotId));
            
            var result = new GroupGraphType(groupEvent.Group!);
            await sender.Send(result);
            return new GroupPayload(result);
        }

        [Authorize(Policy = AuthPolicies.ManageGroup)]
        public async Task<GroupPayload> CreateInvite(
            [GraphQLType(typeof(IdType))] string groupId,
            bool singleUse,
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db,
            [Service] ITopicEventSender sender)
        {
            var id = ID.ToLong<Group>(groupId);
            
            await groupService.CreateInviteAsync(id, singleUse);

            var group = await db.Groups.FindAsync(id);
            if (group == null) throw new Exception("Group not found");
            
            var result = new GroupGraphType(group);
            await sender.Send(result);
            return new GroupPayload(result);
        }

        [Authorize(Policy = AuthPolicies.ManageGroup)]
        public async Task<GroupPayload> DeleteInvite(
            [GraphQLType(typeof(IdType))] string groupId,
            string inviteCode,
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db,
            [Service] ITopicEventSender sender)
        {
            var id = ID.ToLong<Group>(groupId);
            
            var invite = await db.GroupInvites
                .Include(x => x.Group)
                .Where(x => x.Id == inviteCode)
                .Where(x => x.GroupId == id)
                .FirstOrDefaultAsync();
            if (invite == null) throw new Exception("Invalid invite");

            await groupService.DeleteInviteAsync(inviteCode);
            
            var result = new GroupGraphType(invite.Group!);
            await sender.Send(result);
            return new GroupPayload(result);
        }

        [Authorize(Policy = AuthPolicies.ReadGroup)]
        public async Task<GroupPayload> SetAvailable(
            [GraphQLType(typeof(IdType))] string groupId,
            int lengthInSeconds,
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db,
            [Service] ITopicEventSender sender)
        {
            var id = ID.ToLong<Group>(groupId);
            await groupService.SetAvailableFor(id, lengthInSeconds);

            var group = await db.Groups.FindAsync(id);
            if (group == null) throw new Exception("Group not found");
            
            var result = new GroupGraphType(group);
            await sender.Send(result);
            return new GroupPayload(result);
        }
        
        [Authorize(Policy = AuthPolicies.ReadGroup)]
        public async Task<GroupPayload> SetUnavailable(
            [GraphQLType(typeof(IdType))] string groupId,
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db,
            [Service] ITopicEventSender sender)
        {
            var id = ID.ToLong<Group>(groupId);
            await groupService.SetAvailableFor(id, -1);

            var group = await db.Groups.FindAsync(id);
            if (group == null) throw new Exception("Group not found");
            
            var result = new GroupGraphType(group);
            await sender.Send(result);
            return new GroupPayload(result);
        }

        [Authorize(Policy = AuthPolicies.ManageGroup)]
        public async Task<GroupPayload> AddGame(
            [GraphQLType(typeof(IdType))] string groupId,
            [GraphQLType(typeof(IdType))] string gameId,
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db,
            [Service] ITopicEventSender sender)
        {
            await groupService.AddGameToGroupAsync(ID.ToLong<Group>(groupId), ID.ToLong<Game>(gameId));

            var group = await db.Groups.FindAsync(ID.ToLong<Group>(groupId));
            if (group == null) throw new Exception("Group not found");
            
            var result = new GroupGraphType(group);
            await sender.Send(result);
            return new GroupPayload(result);
        }
        

        [Authorize(Policy = AuthPolicies.ManageGroup)]
        public async Task<GroupPayload> RemoveGame(
            [GraphQLType(typeof(IdType))] string groupId,
            [GraphQLType(typeof(IdType))] string gameId,
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db,
            [Service] ITopicEventSender sender)
        {
            await groupService.RemoveGameFromGroupAsync(ID.ToLong<Group>(groupId), ID.ToLong<GroupGame>(gameId));

            var group = await db.Groups.FindAsync(ID.ToLong<Group>(groupId));
            if (group == null) throw new Exception("Group not found");
            
            var result = new GroupGraphType(group);
            await sender.Send(result);
            return new GroupPayload(result);
        }


        [Authorize(Policy = AuthPolicies.ReadGroup)]
        public async Task<GroupPayload> ProposeSession(
            [GraphQLType(typeof(IdType))] string groupId,
            [GraphQLType(typeof(IdType))] string? gameId,
            LocalDateTime[] dateTimes,
            string details,
            [Service] GroupService groupService,
            [Service] DateService dateService,
            [Service] ApplicationDbContext db,
            [Service] ITopicEventSender sender)
        {
            var instants = dateTimes
                .Select(x => dateService.ConvertFromUserLocalTime(x))
                .ToArray();
            
            await groupService.ProposeEventAsync(
                ID.ToLong<Group>(groupId),
                gameId == null ? null : ID.ToLong<GroupGame>(gameId),
                details,
                instants);
            
            var group = await db.Groups.FindAsync(ID.ToLong<Group>(groupId));
            if (group == null) throw new Exception("Group not found");

            var result = new GroupGraphType(group);
            await sender.Send(result);
            return new GroupPayload(result);
        }

        [Authorize(Policy = AuthPolicies.ManageGroup)]
        public async Task<GroupPayload> RemoveGroupMember(
            [GraphQLType(typeof(IdType))] string groupId,
            [GraphQLType(typeof(IdType))] string memberId,
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db,
            [Service] ITopicEventSender sender)
        {
            var (_, userId) = MembershipGraphType.ParseMembershipId(memberId);

            await groupService.RemoveGroupMember(ID.ToLong<Group>(groupId), userId);
            
            var group = await db.Groups.FindAsync(ID.ToLong<Group>(groupId));
            if (group == null) throw new Exception("Group not found");

            var result = new GroupGraphType(group);
            await sender.Send(result);
            return new GroupPayload(result);
        }

        [Authorize(Policy = AuthPolicies.ManageGroup)]
        public async Task<bool> DeleteGroup(
            [GraphQLType(typeof(IdType))] string groupId,
            [Service] GroupService groupService)
        {
            await groupService.DeleteGroup(ID.ToLong<Group>(groupId));

            return true;
        }

        [Authorize(Policy = AuthPolicies.ReadGroup)]
        public async Task<bool> LeaveGroup(
            [GraphQLType(typeof(IdType))] string groupId,
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db,
            [Service] ITopicEventSender sender)
        {
            await groupService.LeaveGroup(ID.ToLong<Group>(groupId));

            var group = await db.Groups.FindAsync(groupId);
            if (group == null) throw new Exception("Group not found");
            await sender.Send(new GroupGraphType(group));
            
            return true;
        }
        
        [Authorize(Policy = AuthPolicies.ManageSession)]
        public async Task<GroupPayload> DeleteSession(
            [GraphQLType(typeof(IdType))] string sessionId,
            [Service] GroupService groupService,
            [Service] ApplicationDbContext db,
            [Service] ITopicEventSender sender)
        {
            var id = ID.ToLong<GroupEvent>(sessionId);
            
            var group = await db.GroupEvents
                .Where(x => x.Id == id)
                .Select(x => x.Group)
                .FirstOrDefaultAsync();
            if (group == null) throw new Exception("Group not found");
            
            await groupService.DeleteEvent(id);
            
            var result = new GroupGraphType(group);
            await sender.Send(result);
            return new GroupPayload(result);
        }

        [Authorize]
        public async Task<GroupPayload> CreateGroup(
            string groupName, 
            string displayName,
            [Service] GroupService groupService)
        {
            if (string.IsNullOrWhiteSpace(displayName)) throw new Exception("Display name can't be blank");
            
            var group = await groupService.CreateGroupAsync(groupName, displayName);
            return new GroupPayload(new GroupGraphType(group));
        }

        [Authorize]
        public async Task<MembershipPayload> EditDisplayName(
            [GraphQLType(typeof(IdType))] string groupId,
            string newName,
            [Service] GroupService groupService
        )
        {
            if (string.IsNullOrWhiteSpace(newName)) throw new Exception("New name can't be blank");
            
            var membership = await groupService.UpdateMemberDisplayNameAsync(ID.ToLong<Group>(groupId), newName);
            if (membership == null) throw new Exception("Can't find membership");
            return new MembershipPayload(new MembershipGraphType(membership));
        }

        [Authorize(Policy = AuthPolicies.ManageSession)]
        public async Task<ProposedSessionPayload> UpdateProposedSession(
            [GraphQLNonNullType] UpdateSessionInput input,
            [Service] GroupService groupService
        )
        {
            var ev = await groupService.UpdateEventAsync(
                ID.ToLong<GroupEvent>(input.SessionId), 
                input.Details, 
                input.GameId.Map(ID.ToLongOptional<GroupGame>));
            
            if (ev == null) throw new Exception("Can't find event");
            
            return new ProposedSessionPayload(new ProposedSessionGraphType(ev));
        }
    }
}