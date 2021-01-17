using System;
using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.Helpers;
using LetsGame.Web.Services.Igdb;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.Services
{
    public class GroupService
    {
        private readonly ApplicationDbContext _db;
        private readonly SlugGenerator _slugGenerator;
        private readonly IgdbClient _igdbClient;

        public GroupService(ApplicationDbContext db, SlugGenerator slugGenerator, IgdbClient igdbClient)
        {
            _db = db;
            _slugGenerator = slugGenerator;
            _igdbClient = igdbClient;
        }

        public Task<Group> FindBySlugAsync(string slug)
        {
            return _db.Groups.FirstOrDefaultAsync(x => x.Slug == slug);
        }

        public Task<string> GetSlugFromGroupNameAsync(string name)
        {
            return _slugGenerator.GenerateWithCheck(name, slug => _db.Groups.AnyAsync(x => x.Slug == slug));
        }

        public async Task<Group> CreateGroupAsync(string groupName, string ownerId, string ownerDisplayName)
        {
            var group = new Group
            {
                Name = groupName,
                Slug = await GetSlugFromGroupNameAsync(groupName)
            };

            _db.Groups.Add(group);
            _db.Memberships.Add(new Membership
            {
                Group = group, 
                UserId = ownerId, 
                DisplayName = ownerDisplayName, 
                Role = GroupRole.Owner
            });

            await _db.SaveChangesAsync();

            return group;
        }

        public async Task<GroupGame> AddGameToGroupAsync(long groupId, long igdbId)
        {
            var groupGame = await _db.GroupGames
                .FirstOrDefaultAsync(x => x.GroupId == groupId &&
                                          x.IgdbId == igdbId);
            
            if (groupGame != null) return groupGame;
            
            var game = await _igdbClient.GetGameAsync(igdbId);
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

            return groupGame;
        }

        public async Task AddSlotVoteAsync(long slotId, string voterId)
        {
            var slot = await _db.GroupEventSlots
                .Include(x => x.Votes)
                .FirstOrDefaultAsync(x => x.Id == slotId);
            
            if (slot != null && !slot.Votes.Any(v => v.VoterId == voterId))
            {
                _db.GroupEventSlotVotes.Add(new GroupEventSlotVote
                {
                    Slot = slot,
                    VoterId = voterId,
                    VotedAtUtc = DateTime.UtcNow
                });
                
                await _db.SaveChangesAsync();
            }
        }

        public async Task RemoveSlotVoteAsync(long slotId, string voterId)
        {
            var vote = await _db.GroupEventSlotVotes
                .FirstOrDefaultAsync(x => x.SlotId == slotId && x.VoterId == voterId);

            if (vote != null)
            {
                _db.GroupEventSlotVotes.Remove(vote);
                await _db.SaveChangesAsync();
            }
        }

        public async Task PickSlotAsync(long slotId)
        {
            var slot = await _db.GroupEventSlots
                .Include(x => x.Event)
                .FirstOrDefaultAsync(x => x.Id == slotId);

            slot.Event.ChosenDateAndTimeUtc = slot.ProposedDateAndTimeUtc;
            await _db.SaveChangesAsync();
        }

        public async Task<GroupInvite> CreateInviteAsync(long groupId, bool singleUse)
        {
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

            return invite;
        }

        public async Task DeleteInviteAsync(string id)
        {
            var invite = await _db.GroupInvites.FindAsync(id);
            if (invite != null) _db.GroupInvites.Remove(invite);
            
            await _db.SaveChangesAsync();
        }

        public async Task<Group> AcceptInviteAsync(string userId, string displayName, string inviteId)
        {
            var invite = await _db.GroupInvites.Include(x => x.Group).FirstOrDefaultAsync(x => x.Id == inviteId);

            _db.Memberships.Add(new Membership
            {
                UserId = userId,
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
    }
}