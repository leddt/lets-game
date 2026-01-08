using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using LetsGame.Web.Data;
using LetsGame.Web.Helpers;
using LetsGame.Web.Services.Igdb;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace LetsGame.Web.Services
{
    public class GroupService(
        ApplicationDbContext db,
        SlugGenerator slugGenerator,
        IGameSearcher gameSearcher,
        ICurrentUserAccessor currentUserAccessor,
        UserManager<AppUser> userManager,
        INotificationService notificationService
    )
    {
        public Task<Group?> FindBySlugAsync(string slug)
        {
            return db.Groups.FirstOrDefaultAsync(x => x.Slug == slug &&
                                                       x.Memberships!.Any(m => m.UserId == CurrentUserId));
        }

        public async Task<Group> CreateGroupAsync(string groupName, string ownerDisplayName)
        {
            if (CurrentUserId == null) throw new InvalidOperationException("No current user");
            
            var group = new Group
            {
                Name = groupName,
                Slug = await CreateSlugFromGroupNameAsync(groupName)
            };

            db.Groups.Add(group);
            db.Memberships.Add(new Membership
            {
                Group = group, 
                UserId = CurrentUserId, 
                DisplayName = ownerDisplayName, 
                Role = GroupRole.Owner
            });

            await db.SaveChangesAsync();

            return group;
        }

        public async Task AddGameToGroupAsync(long groupId, long igdbId)
        {
            await EnsureIsGroupOwnerAsync(groupId);

            var groupGame = await db.GroupGames
                .FirstOrDefaultAsync(x => x.GroupId == groupId &&
                                          x.IgdbId == igdbId);
            
            if (groupGame != null) return;
            
            var game = await gameSearcher.GetGameAsync(igdbId);
            if (game == null) throw new ArgumentException("Game not found", nameof(igdbId));

            groupGame = new GroupGame
            {
                GroupId = groupId,
                IgdbId = game.Id,
                Name = game.Name,
                IgdbImageId = game.MainImage?.ImageId
            };

            db.GroupGames.Add(groupGame);
            await db.SaveChangesAsync();
        }

        public async Task RemoveGameFromGroupAsync(long groupId, long groupGameId)
        {
            await EnsureIsGroupOwnerAsync(groupId);
            
            var groupGame = await db.GroupGames
                .FirstOrDefaultAsync(x => x.GroupId == groupId &&
                                          x.Id == groupGameId);
            
            if (groupGame == null) return;

            var events = await db.GroupEvents
                .Where(x => x.GroupId == groupId && 
                            x.GameId == groupGameId)
                .ToListAsync();

            db.GroupEvents.RemoveRange(events);
            db.GroupGames.Remove(groupGame);
            
            await db.SaveChangesAsync();
        }

        public async Task AddSlotVoteAsync(long slotId)
        {
            if (CurrentUserId == null) throw new InvalidOperationException("No current user");
            
            var slot = await db.GroupEventSlots
                .Include(x => x.Votes)
                .Where(x => x.Event!.Group!.Memberships!.Any(m => m.UserId == CurrentUserId))
                .FirstOrDefaultAsync(x => x.Id == slotId);
            
            if (slot != null && slot.Votes!.All(v => v.VoterId != CurrentUserId))
            {
                db.GroupEventSlotVotes.Add(new GroupEventSlotVote
                {
                    Slot = slot,
                    VoterId = CurrentUserId,
                    VotedAt = SystemClock.Instance.GetCurrentInstant()
                });
                
                var cantPlay = await db.GroupEventCantPlays.FirstOrDefaultAsync(x => x.EventId == slot.EventId && x.UserId == CurrentUserId);
                if (cantPlay != null)
                    db.GroupEventCantPlays.Remove(cantPlay);
                
                await db.SaveChangesAsync();

                await CheckForAllVotesReceived(slot.EventId);
            }
        }

        public async Task RemoveSlotVoteAsync(long slotId)
        {
            var vote = await db.GroupEventSlotVotes
                .FirstOrDefaultAsync(x => x.SlotId == slotId && x.VoterId == CurrentUserId);

            if (vote != null)
            {
                db.GroupEventSlotVotes.Remove(vote);
                await db.SaveChangesAsync();
            }
        }

        public async Task SetCantPlayAsync(long eventId)
        {
            if (CurrentUserId == null) throw new InvalidOperationException("No current user");
            
            var isGroupMember = await db.GroupEvents
                .Where(e => e.Group!.Memberships!.Any(m => m.UserId == CurrentUserId))
                .AnyAsync(e => e.Id == eventId);
            if (!isGroupMember) throw new InvalidOperationException("Not group member");
            
            var votes = await db.GroupEventSlotVotes.Where(x => x.Slot!.EventId == eventId && x.VoterId == CurrentUserId).ToListAsync();
            if (votes.Any()) db.GroupEventSlotVotes.RemoveRange(votes);

            var exists = await db.GroupEventCantPlays.AnyAsync(x => x.EventId == eventId && x.UserId == CurrentUserId);
            if (!exists)
            {
                db.GroupEventCantPlays.Add(new GroupEventCantPlay {EventId = eventId, UserId = CurrentUserId});
            }

            await db.SaveChangesAsync();

            await CheckForAllVotesReceived(eventId);
        }

        public async Task PickSlotAsync(long slotId)
        {
            if (CurrentUserId == null) throw new InvalidOperationException("No current user");
            
            var slot = await db.GroupEventSlots
                .Include(x => x.Event)
                .Where(x => x.Event!.CreatorId == CurrentUserId || x.Event.Group!.Memberships!.Any(m => m.Role == GroupRole.Owner && m.UserId == CurrentUserId))
                .FirstAsync(x => x.Id == slotId);

            slot.Event!.ChosenTime = slot.ProposedTime;
            await db.SaveChangesAsync();

            await notificationService.NotifySlotPicked(slot.Event, CurrentUserId);
        }

        public async Task CreateInviteAsync(long groupId, bool singleUse)
        {
            await EnsureIsGroupOwnerAsync(groupId);
            
            string id;
            
            do
            {
                id = RandomHelper.GetRandomIdentifier(8);
            } while (await db.GroupInvites.AnyAsync(x => x.Id == id));

            var invite = new GroupInvite
            {
                Id = id,
                GroupId = groupId,
                IsSingleUse = singleUse
            };

            db.GroupInvites.Add(invite);
            await db.SaveChangesAsync();
        }

        public async Task DeleteInviteAsync(string id)
        {
            var invite = await db.GroupInvites
                .Where(x => x.Group!.Memberships!.Any(m => m.Role == GroupRole.Owner && m.UserId == CurrentUserId))
                .FirstOrDefaultAsync(x => x.Id == id);
            
            if (invite != null) db.GroupInvites.Remove(invite);
            
            await db.SaveChangesAsync();
        }

        public async Task RemoveGroupMember(long groupId, string memberId)
        {
            var member = await db.Memberships
                .Where(x => x.Group!.Memberships!.Any(m => m.UserId == CurrentUserId && m.Role == GroupRole.Owner))
                .FirstOrDefaultAsync(x => x.GroupId == groupId && x.UserId == memberId && x.Role != GroupRole.Owner);
            
            if (member != null) await DeleteMember(member);
        }

        public async Task LeaveGroup(long groupId)
        {
            var member = await db.Memberships
                .Where(m => m.GroupId == groupId && m.UserId == CurrentUserId)
                .FirstAsync();

            if (member.Role == GroupRole.Owner)
                throw new InvalidOperationException("Owner can't leave group");

            await DeleteMember(member);
        }

        private async Task CheckForAllVotesReceived(long eventId)
        {
            var groupEvent = await db.GroupEvents.FindAsync(eventId)
                             ?? throw new Exception("Event not found");
            if (groupEvent.CreatorId != CurrentUserId && 
                groupEvent.ChosenTime == null && 
                groupEvent.AllVotesInNotificationSentAt == null)
            {
                var groupMemberCount = await db.Groups
                    .Where(x => x.Events!.Any(e => e.Id == groupEvent.Id))
                    .SelectMany(x => x.Memberships!)
                    .CountAsync();

                var voterIds = db.GroupEvents
                    .Where(x => x.Id == groupEvent.Id)
                    .SelectMany(x => x.Slots!)
                    .SelectMany(x => x.Votes!)
                    .Select(x => x.VoterId);
                    
                var cantPlayIds = db.GroupEvents
                    .Where(x => x.Id == groupEvent.Id)
                    .SelectMany(x => x.CantPlays!)
                    .Select(x => x.UserId);
                    
                var distinctVoterCount = await voterIds
                    .Union(cantPlayIds)
                    .Distinct()
                    .CountAsync();

                if (groupMemberCount == distinctVoterCount)
                {
                    groupEvent.AllVotesInNotificationSentAt = SystemClock.Instance.GetCurrentInstant();
                    await db.SaveChangesAsync();
                        
                    await notificationService.NotifyAllVotesIn(groupEvent);
                }
            }
        }

        private async Task DeleteMember(Membership member)
        {
            db.Memberships.Remove(member);

            var events = await db.GroupEvents
                .Where(x => x.GroupId == member.GroupId && x.CreatorId == member.UserId)
                .ToListAsync();
            events.ForEach(ev => ev.CreatorId = null);

            var votes = await db.GroupEventSlotVotes
                .Where(x => x.Slot!.Event!.GroupId == member.GroupId && x.VoterId == member.UserId)
                .ToListAsync();
            if (votes.Any())
                db.GroupEventSlotVotes.RemoveRange(votes);

            var cantPlays = await db.GroupEventCantPlays
                .Where(x => x.Event!.GroupId == member.GroupId && x.UserId == member.UserId)
                .ToListAsync();
            if (cantPlays.Any())
                db.GroupEventCantPlays.RemoveRange(cantPlays);

            await db.SaveChangesAsync();
        }

        public async Task<Group> AcceptInviteAsync(string displayName, string inviteId)
        {
            if (CurrentUserId == null) throw new InvalidOperationException("No current user");
            
            var invite = await db.GroupInvites
                .Include(x => x.Group).ThenInclude(x => x!.Memberships)
                .FirstAsync(x => x.Id == inviteId);

            if (invite.Group!.Memberships!.Any(m => m.UserId == CurrentUserId))
                return invite.Group;

            db.Memberships.Add(new Membership
            {
                UserId = CurrentUserId,
                GroupId = invite.GroupId,
                DisplayName = displayName,
                Role = GroupRole.Member
            });

            if (invite.IsSingleUse)
            {
                db.GroupInvites.Remove(invite);
            }

            await db.SaveChangesAsync();

            return invite.Group;
        }

        public async Task ProposeEventAsync(long groupId, long? gameId, string details, Instant[] slots)
        {
            if (CurrentUserId == null) throw new InvalidOperationException("No current user");
            
            GroupGame? game = null;
            if (gameId.HasValue)
            {
                game = await db.GroupGames.FirstOrDefaultAsync(x => x.GroupId == groupId && x.Id == gameId);
                if (game == null) throw new InvalidOperationException($"Unknown game");
            }

            var groupEvent = new GroupEvent
            {
                GroupId = groupId,
                Game = game,
                Details = details,
                CreatorId = CurrentUserId,
                Slots = slots
                    .Select(s => new GroupEventSlot
                    {
                        ProposedTime = s,
                        Votes = new List<GroupEventSlotVote>
                        {
                            new() { VoterId = CurrentUserId }
                        }
                    })
                    .ToList()
            };
            
            db.GroupEvents.Add(groupEvent);
            await db.SaveChangesAsync();

            await notificationService.NotifyEventAdded(groupEvent);
        }

        public async Task SetAvailableFor(long groupId, int lengthInSeconds)
        {
            var member = await db.Memberships
                .Include(x => x.Group)
                .Where(x => x.GroupId == groupId)
                .Where(x => x.UserId == CurrentUserId)
                .FirstOrDefaultAsync();
            
            if (member != null)
            {
                var wasAvailable = member.IsAvailableNow();

                var now = SystemClock.Instance.GetCurrentInstant();
                member.AvailableUntil = now + Duration.FromSeconds(lengthInSeconds);
                await db.SaveChangesAsync();

                if (member.IsAvailableNow() &&
                    !wasAvailable &&
                    (member.AvailabilityNotificationSentAt == null ||
                     member.AvailabilityNotificationSentAt < now - Duration.FromHours(1)))
                {
                    member.AvailabilityNotificationSentAt = now;
                    await db.SaveChangesAsync();

                    await notificationService.NotifyMemberAvailable(member.Group!, member);
                }
            }
        }

        public async Task DeleteEvent(long eventId)
        {
            var ev = await db.GroupEvents
                .Where(x => x.CreatorId == CurrentUserId ||
                            x.Group!.Memberships!.Any(m => m.Role == GroupRole.Owner && m.UserId == CurrentUserId))
                .FirstOrDefaultAsync(x => x.Id == eventId);

            if (ev != null)
            {
                db.GroupEvents.Remove(ev);
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteGroup(long groupId)
        {
            var group = await db.Groups
                .Include(x => x.Events)
                .FirstOrDefaultAsync(x =>
                    x.Id == groupId &&
                    x.Memberships!.Any(m => m.Role == GroupRole.Owner &&
                                           m.User!.Id == CurrentUserId));

            if (group != null)
            {
                if (group.Events!.Any()) 
                    db.GroupEvents.RemoveRange(group.Events!);
                
                db.Groups.Remove(group);
                await db.SaveChangesAsync();
            }
        }

        public async Task<Membership?> UpdateMemberDisplayNameAsync(long groupId, string newName)
        {
            var membership = await db.Memberships
                .FirstOrDefaultAsync(x =>
                    x.GroupId == groupId &&
                    x.UserId == CurrentUserId);

            if (membership != null)
            {
                membership.DisplayName = newName;
                await db.SaveChangesAsync();
            }

            return membership;
        }

        private string? CurrentUserId => currentUserAccessor.CurrentUser != null 
            ? userManager.GetUserId(currentUserAccessor.CurrentUser) 
            : null;

        private Task<string> CreateSlugFromGroupNameAsync(string name)
        {
            return slugGenerator.GenerateWithCheck(name, slug => db.Groups.AnyAsync(x => x.Slug == slug));
        }

        private async Task EnsureIsGroupOwnerAsync(long groupId)
        {
            var isGroupOwner = await db.Groups
                .AnyAsync(x => x.Id == groupId &&
                               x.Memberships!.Any(m =>
                                   m.Role == GroupRole.Owner &&
                                   m.UserId == CurrentUserId));
            
            if (!isGroupOwner) throw new InvalidOperationException("Not group owner");
        }

        public async Task<GroupEvent?> UpdateEventAsync(long eventId, string? details, Optional<long?> gameId)
        {
            var ev = await db.GroupEvents
                .Where(x => x.CreatorId == CurrentUserId ||
                            x.Group!.Memberships!.Any(m => m.Role == GroupRole.Owner && m.UserId == CurrentUserId))
                .FirstOrDefaultAsync(x => x.Id == eventId);

            if (ev == null) return null;
            
            if (details != null)
            {
                ev.Details = details;
            }
            
            if (gameId.HasValue)
            {
                GroupGame? game = null;
                
                if (gameId.Value != null)
                {
                    game = await db.GroupGames.FirstOrDefaultAsync(x => x.GroupId == ev.GroupId && x.Id == gameId.Value);
                    if (game == null) throw new InvalidOperationException("Unknown game");
                }

                ev.GameId = game?.Id;
            }
            
            await db.SaveChangesAsync();

            return ev;
        }
    }
}