﻿using System;
using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.Data;
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

        public async Task<Group> CreateGroupAsync(string name, string ownerId)
        {
            var group = new Group
            {
                Name = name,
                Slug = await GetSlugFromGroupNameAsync(name)
            };

            _db.Groups.Add(group);
            _db.Memberships.Add(new Membership {Group = group, UserId = ownerId, Role = GroupRole.Owner});

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

        public async Task AddSlotVote(long slotId, string voterId)
        {
            var slot = await _db.GroupEventSlots.Include(x => x.Votes).FirstOrDefaultAsync(x => x.Id == slotId);
            
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
    }
}