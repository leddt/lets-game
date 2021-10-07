using System;
using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.Helpers;
using LetsGame.Web.Services.Igdb;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.Services
{
    public class GroupService
    {
        private readonly ApplicationDbContext _db;
        private readonly SlugGenerator _slugGenerator;
        private readonly IGameSearcher _gameSearcher;
        private readonly ICurrentUserAccessor _currentUserAccessor;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;

        public GroupService(
            ApplicationDbContext db, 
            SlugGenerator slugGenerator, 
            IGameSearcher gameSearcher, 
            ICurrentUserAccessor currentUserAccessor, 
            UserManager<AppUser> userManager,
            INotificationService notificationService)
        {
            _db = db;
            _slugGenerator = slugGenerator;
            _gameSearcher = gameSearcher;
            _currentUserAccessor = currentUserAccessor;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public Task<Group> FindBySlugAsync(string slug)
        {
            return _db.Groups.FirstOrDefaultAsync(x => x.Slug == slug &&
                                                       x.Memberships.Any(m => m.UserId == CurrentUserId));
        }

        public async Task<Group> CreateGroupAsync(string groupName, string ownerDisplayName)
        {
            var group = new Group
            {
                Name = groupName,
                Slug = await CreateSlugFromGroupNameAsync(groupName)
            };

            _db.Groups.Add(group);
            _db.Memberships.Add(new Membership
            {
                Group = group, 
                UserId = CurrentUserId, 
                DisplayName = ownerDisplayName, 
                Role = GroupRole.Owner
            });

            await _db.SaveChangesAsync();

            return group;
        }

        public async Task AddGameToGroupAsync(long groupId, long igdbId)
        {
            await EnsureIsGroupOwnerAsync(groupId);

            var groupGame = await _db.GroupGames
                .FirstOrDefaultAsync(x => x.GroupId == groupId &&
                                          x.IgdbId == igdbId);
            
            if (groupGame != null) return;
            
            var game = await _gameSearcher.GetGameAsync(igdbId);
            if (game == null) throw new ArgumentException("Game not found", nameof(igdbId));

            groupGame = new GroupGame
            {
                GroupId = groupId,
                IgdbId = game.Id,
                Name = game.Name,
                IgdbImageId = game.MainImage?.ImageId
            };

            _db.GroupGames.Add(groupGame);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveGameFromGroupAsync(long groupId, long groupGameId)
        {
            await EnsureIsGroupOwnerAsync(groupId);
            
            var groupGame = await _db.GroupGames
                .FirstOrDefaultAsync(x => x.GroupId == groupId &&
                                          x.Id == groupGameId);
            
            if (groupGame == null) return;

            _db.GroupGames.Remove(groupGame);
            await _db.SaveChangesAsync();
        }

        public async Task AddSlotVoteAsync(long slotId)
        {
            var slot = await _db.GroupEventSlots
                .Include(x => x.Votes)
                .Where(x => x.Event.Group.Memberships.Any(m => m.UserId == CurrentUserId))
                .FirstOrDefaultAsync(x => x.Id == slotId);
            
            if (slot != null && !slot.Votes.Any(v => v.VoterId == CurrentUserId))
            {
                _db.GroupEventSlotVotes.Add(new GroupEventSlotVote
                {
                    Slot = slot,
                    VoterId = CurrentUserId,
                    VotedAtUtc = DateTime.UtcNow
                });
                
                var cantPlay = await _db.GroupEventCantPlays.FirstOrDefaultAsync(x => x.EventId == slot.EventId && x.UserId == CurrentUserId);
                if (cantPlay != null)
                    _db.GroupEventCantPlays.Remove(cantPlay);
                
                await _db.SaveChangesAsync();

                var groupEvent = await _db.GroupEvents.FindAsync(slot.EventId);
                if (groupEvent.CreatorId != CurrentUserId && 
                    groupEvent.ChosenDateAndTimeUtc == null && 
                    groupEvent.AllVotesInNotificationSentAtUtc == null)
                {
                    var groupMemberCount = await _db.Groups
                        .Where(x => x.Events.Any(e => e.Id == groupEvent.Id))
                        .SelectMany(x => x.Memberships)
                        .CountAsync();

                    var distinctVoterCount = await _db.GroupEvents
                        .Where(x => x.Id == groupEvent.Id)
                        .SelectMany(x => x.Slots)
                        .SelectMany(x => x.Votes)
                        .Select(x => x.Voter)
                        .Distinct()
                        .CountAsync();

                    if (groupMemberCount == distinctVoterCount)
                    {
                        groupEvent.AllVotesInNotificationSentAtUtc = DateTime.UtcNow;
                        await _db.SaveChangesAsync();
                        
                        await _notificationService.NotifyAllVotesIn(groupEvent);
                    }
                }
            }
        }

        public async Task RemoveSlotVoteAsync(long slotId)
        {
            var vote = await _db.GroupEventSlotVotes
                .FirstOrDefaultAsync(x => x.SlotId == slotId && x.VoterId == CurrentUserId);

            if (vote != null)
            {
                _db.GroupEventSlotVotes.Remove(vote);
                await _db.SaveChangesAsync();
            }
        }

        public async Task SetCantPlayAsync(long eventId)
        {
            var isGroupMember = await _db.GroupEvents
                .Where(e => e.Group.Memberships.Any(m => m.UserId == CurrentUserId))
                .AnyAsync(e => e.Id == eventId);
            if (!isGroupMember) throw new InvalidOperationException("Not group member");
            
            var votes = await _db.GroupEventSlotVotes.Where(x => x.Slot.EventId == eventId && x.VoterId == CurrentUserId).ToListAsync();
            if (votes.Any()) _db.GroupEventSlotVotes.RemoveRange(votes);

            var exists = await _db.GroupEventCantPlays.AnyAsync(x => x.EventId == eventId && x.UserId == CurrentUserId);
            if (!exists)
            {
                _db.GroupEventCantPlays.Add(new GroupEventCantPlay {EventId = eventId, UserId = CurrentUserId});
            }

            await _db.SaveChangesAsync();
        }

        public async Task PickSlotAsync(long slotId)
        {
            var slot = await _db.GroupEventSlots
                .Include(x => x.Event)
                .Where(x => x.Event.CreatorId == CurrentUserId || x.Event.Group.Memberships.Any(m => m.Role == GroupRole.Owner && m.UserId == CurrentUserId))
                .FirstOrDefaultAsync(x => x.Id == slotId);

            slot.Event.ChosenDateAndTimeUtc = slot.ProposedDateAndTimeUtc;
            await _db.SaveChangesAsync();

            await _notificationService.NotifySlotPicked(slot.Event, CurrentUserId);
        }

        public async Task CreateInviteAsync(long groupId, bool singleUse)
        {
            await EnsureIsGroupOwnerAsync(groupId);
            
            string id;
            
            do
            {
                id = RandomHelper.GetRandomIdentifier(8);
            } while (await _db.GroupInvites.AnyAsync(x => x.Id == id));

            var invite = new GroupInvite
            {
                Id = id,
                GroupId = groupId,
                IsSingleUse = singleUse
            };

            _db.GroupInvites.Add(invite);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteInviteAsync(string id)
        {
            var invite = await _db.GroupInvites
                .Where(x => x.Group.Memberships.Any(m => m.Role == GroupRole.Owner && m.UserId == CurrentUserId))
                .FirstOrDefaultAsync(x => x.Id == id);
            
            if (invite != null) _db.GroupInvites.Remove(invite);
            
            await _db.SaveChangesAsync();
        }

        public async Task RemoveGroupMember(long groupId, string memberId)
        {
            var member = await _db.Memberships
                .Where(x => x.Group.Memberships.Any(m => m.UserId == CurrentUserId && m.Role == GroupRole.Owner))
                .FirstOrDefaultAsync(x => x.GroupId == groupId && x.UserId == memberId && x.Role != GroupRole.Owner);
            
            if (member != null) await DeleteMember(member);
        }

        public async Task LeaveGroup(long groupId)
        {
            var member = await _db.Memberships
                .Where(m => m.GroupId == groupId && m.UserId == CurrentUserId)
                .FirstOrDefaultAsync();

            if (member.Role == GroupRole.Owner)
                throw new InvalidOperationException("Owner can't leave group");

            await DeleteMember(member);
        }

        private async Task DeleteMember(Membership member)
        {
            _db.Memberships.Remove(member);

            var events = await _db.GroupEvents
                .Where(x => x.GroupId == member.GroupId && x.CreatorId == member.UserId)
                .ToListAsync();
            events.ForEach(ev => ev.CreatorId = null);

            var votes = await _db.GroupEventSlotVotes
                .Where(x => x.Slot.Event.GroupId == member.GroupId && x.VoterId == member.UserId)
                .ToListAsync();
            if (votes.Any())
                _db.GroupEventSlotVotes.RemoveRange(votes);

            var cantPlays = await _db.GroupEventCantPlays
                .Where(x => x.Event.GroupId == member.GroupId && x.UserId == member.UserId)
                .ToListAsync();
            if (cantPlays.Any())
                _db.GroupEventCantPlays.RemoveRange(cantPlays);

            await _db.SaveChangesAsync();
        }

        public async Task<Group> AcceptInviteAsync(string displayName, string inviteId)
        {
            var invite = await _db.GroupInvites
                .Include(x => x.Group).ThenInclude(x => x.Memberships)
                .FirstOrDefaultAsync(x => x.Id == inviteId);

            if (invite.Group.Memberships.Any(m => m.UserId == CurrentUserId))
                return invite.Group;

            _db.Memberships.Add(new Membership
            {
                UserId = CurrentUserId,
                GroupId = invite.GroupId,
                DisplayName = displayName,
                Role = GroupRole.Member
            });

            if (invite.IsSingleUse)
            {
                _db.GroupInvites.Remove(invite);
            }

            await _db.SaveChangesAsync();

            return invite.Group;
        }

        public async Task ProposeEventAsync(long groupId, long? gameId, string details, DateTime[] slotsUtc)
        {
            GroupGame game = null;
            if (gameId.HasValue)
            {
                game = await _db.GroupGames.FirstOrDefaultAsync(x => x.GroupId == groupId && x.Id == gameId);
                if (game == null) throw new InvalidOperationException($"Unknown game");
            }

            var groupEvent = new GroupEvent
            {
                GroupId = groupId,
                Game = game,
                Details = details,
                CreatorId = CurrentUserId,
                Slots = slotsUtc
                    .Select(dt => new GroupEventSlot
                    {
                        ProposedDateAndTimeUtc = dt
                    })
                    .ToList()
            };
            
            _db.GroupEvents.Add(groupEvent);
            await _db.SaveChangesAsync();

            await _notificationService.NotifyEventAdded(groupEvent);
        }

        public async Task SetAvailableFor(long groupId, int lengthInSeconds)
        {
            var member = await _db.Memberships
                .Include(x => x.Group)
                .Where(x => x.GroupId == groupId)
                .Where(x => x.UserId == CurrentUserId)
                .FirstOrDefaultAsync();
            
            if (member != null)
            {
                var wasAvailable = member.IsAvailableNow();

                var utcNow = DateTime.UtcNow;
                member.AvailableUntilUtc = utcNow + TimeSpan.FromSeconds(lengthInSeconds);
                await _db.SaveChangesAsync();

                if (member.IsAvailableNow() &&
                    !wasAvailable &&
                    (member.AvailabilityNotificationSentAtUtc == null ||
                     member.AvailabilityNotificationSentAtUtc < utcNow - TimeSpan.FromHours(1)))
                {
                    member.AvailabilityNotificationSentAtUtc = utcNow;
                    await _db.SaveChangesAsync();

                    await _notificationService.NotifyMemberAvailable(member.Group, member);
                }
            }
        }

        public async Task DeleteEvent(long eventId)
        {
            var ev = await _db.GroupEvents
                .Where(x => x.CreatorId == CurrentUserId ||
                            x.Group.Memberships.Any(m => m.Role == GroupRole.Owner && m.UserId == CurrentUserId))
                .FirstOrDefaultAsync(x => x.Id == eventId);

            if (ev != null)
            {
                _db.GroupEvents.Remove(ev);
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteGroup(long groupId)
        {
            var group = await _db.Groups
                .Include(x => x.Events)
                .FirstOrDefaultAsync(x =>
                    x.Id == groupId &&
                    x.Memberships.Any(m => m.Role == GroupRole.Owner &&
                                           m.User.Id == CurrentUserId));

            if (group != null)
            {
                if (group.Events.Any()) 
                    _db.GroupEvents.RemoveRange(group.Events);
                
                _db.Groups.Remove(group);
                await _db.SaveChangesAsync();
            }
        }

        private string CurrentUserId => _userManager.GetUserId(_currentUserAccessor.CurrentUser);

        private Task<string> CreateSlugFromGroupNameAsync(string name)
        {
            return _slugGenerator.GenerateWithCheck(name, slug => _db.Groups.AnyAsync(x => x.Slug == slug));
        }

        private async Task EnsureIsGroupOwnerAsync(long groupId)
        {
            var isGroupOwner = await _db.Groups
                .AnyAsync(x => x.Id == groupId &&
                               x.Memberships.Any(m =>
                                   m.Role == GroupRole.Owner &&
                                   m.UserId == CurrentUserId));
            
            if (!isGroupOwner) throw new InvalidOperationException("Not group owner");
        }
    }
}