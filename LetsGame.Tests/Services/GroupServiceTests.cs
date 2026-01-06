using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMoqCore;
using LetsGame.Web.Data;
using LetsGame.Web.Services;
using LetsGame.Web.Services.Igdb;
using LetsGame.Web.Services.Igdb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Moq;
using NodaTime;
using Xunit;

namespace LetsGame.Tests.Services
{
    public class GroupServiceTests
    {
        private readonly AutoMoqer _autoMocker = new();
        
        private readonly ApplicationDbContext _db;
        private readonly AppUser _currentUser;

        private readonly GroupService _sut;

        public GroupServiceTests()
        {
            // Required setup for slug generator
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            _db = TestUtils.GetInMemoryDbContext();

            _currentUser = new AppUser();
            _db.Users.Add(_currentUser);
            _db.SaveChanges();
            
            var config = TestUtils.CreateConfiguration(new()
            {
                {"LocalTimezone", TestConfiguration.Timezone}
            });
            
            _autoMocker.SetInstance(_db);
            _autoMocker.SetInstance(new UserManager<AppUser>(
                new UserStore<AppUser>(_db),
                null, null, null, null, null, null, null, null));
            _autoMocker.SetInstance(new DateService(config, DateTimeZoneProviders.Bcl));

            _autoMocker.GetMock<ICurrentUserAccessor>()
                .Setup(x => x.CurrentUser)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new []
                {
                    new Claim(ClaimTypes.NameIdentifier, _currentUser.Id)
                })));
            
            _sut = _autoMocker.Create<GroupService>();
        }

        [Fact]
        public async Task FindGroupBySlugFindsTheUsersGroup()
        {
            _db.Groups.Add(new Group
            {
                Slug = "test",
                Members = new List<AppUser> {_currentUser}
            });
            await _db.SaveChangesAsync();

            var group = await _sut.FindBySlugAsync("test");

            Assert.NotNull(group);
        }

        [Fact]
        public async Task FindGroupBySlugDoesNotReturnGroupIfUserDoesNotBelongToIt()
        {
            _db.Groups.Add(new Group
            {
                Slug = "test",
            });
            await _db.SaveChangesAsync();

            var group = await _sut.FindBySlugAsync("test");

            Assert.Null(group);
        }

        [Fact]
        public async Task CreateGroupCreatesTheGroupAndAssignsTheCurrentUserAsOwner()
        {
            await _sut.CreateGroupAsync("Test group", "Display name");

            var group = await _db.Groups.SingleAsync();
            
            Assert.Equal("Test group", group.Name);
            Assert.Equal("test-group", group.Slug);
            var membership = Assert.Single(group.Memberships);
            Assert.Equal(_currentUser, membership.User);
            Assert.Equal(GroupRole.Owner, membership.Role);
        }

        [Fact]
        public async Task AddGameToGroupFetchesGameDetailsFromSearcher()
        {
            _autoMocker.GetMock<IGameSearcher>()
                .Setup(x => x.GetGameAsync(42))
                .ReturnsAsync(new Game {Id = 42, Name = "Test game", Screenshots = new[] {new Image {ImageId = "test-img"}}});

            var group = await CreateEmptyTestGroup();

            await _sut.AddGameToGroupAsync(group.Id, 42);

            var game = await _db.GroupGames.FirstAsync();

            Assert.Equal(group.Id, game.GroupId);
            Assert.Equal(42, game.IgdbId);
            Assert.Equal("Test game", game.Name);
            Assert.Equal("test-img", game.IgdbImageId);
        }

        [Fact]
        public async Task AddGameToGroupThrowsIfUserIsNotOwnerOfGroup()
        {
            var group = await CreateEmptyTestGroup();
            group.Memberships.First().Role = GroupRole.Member;
            await _db.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddGameToGroupAsync(group.Id, 42));
        }

        [Fact]
        public async Task AddGameToGroupThrowsIfGameNotFound()
        {
            var group = await CreateEmptyTestGroup();
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddGameToGroupAsync(group.Id, 42));
        }

        [Fact]
        public async Task AddGameToGroupDoesNothingIfGroupAlreadyHasThisGame()
        {
            var group = await CreateEmptyTestGroup();
            group.Games = new List<GroupGame> {new() {IgdbId = 42}};
            await _db.SaveChangesAsync();
            
            await _sut.AddGameToGroupAsync(group.Id, 42);

            Assert.Single(group.Games);
        }

        [Fact]
        public async Task AddSlotVoteAddsVoteForCurrentUser()
        {
            var group = await CreateFilledTestGroup();
            var slot = group.Events.First().Slots.First();

            await _sut.AddSlotVoteAsync(slot.Id);

            slot = await _db.GroupEventSlots
                .Include(x => x.Votes).ThenInclude(x => x.Voter)
                .FirstOrDefaultAsync(x => x.Id == slot.Id);
            
            var vote = Assert.Single(slot.Votes);
            Assert.Equal(_currentUser, vote.Voter);
        }

        [Fact]
        public async Task AddSlotVoteDoesNothingIfUserNotMemberOfGroup()
        {
            var group = await CreateFilledTestGroup();
            group.Memberships.Clear();
            await _db.SaveChangesAsync();
            
            var slot = group.Events.First().Slots.First();
            
            await _sut.AddSlotVoteAsync(slot.Id);
            
            slot = await _db.GroupEventSlots
                .Include(x => x.Votes).ThenInclude(x => x.Voter)
                .FirstOrDefaultAsync(x => x.Id == slot.Id);
            
            Assert.Empty(slot.Votes);
        }

        [Fact]
        public async Task AddSlotVoteDoesNothingIfUserAlreadyVoted()
        {
            var group = await CreateFilledTestGroup();
            var slot = group.Events.First().Slots.First();
            slot.Votes = new List<GroupEventSlotVote> {new() {Voter = _currentUser}};
            await _db.SaveChangesAsync();

            await _sut.AddSlotVoteAsync(slot.Id);
            
            slot = await _db.GroupEventSlots
                .Include(x => x.Votes).ThenInclude(x => x.Voter)
                .FirstOrDefaultAsync(x => x.Id == slot.Id);
            
            Assert.Single(slot.Votes);
        }

        [Fact]
        public async Task AddSlotVoteRemovesCantPlay()
        {
            var group = await CreateFilledTestGroup();
            var slot = group.Events.First().Slots.First();
            slot.Event.CantPlays = new List<GroupEventCantPlay> {new() {User = _currentUser}};
            await _db.SaveChangesAsync();
            
            await _sut.AddSlotVoteAsync(slot.Id);
            
            Assert.Empty(slot.Event.CantPlays);
        }

        private async Task<Group> CreateEmptyTestGroup()
        {
            var group = new Group
            {
                Memberships = new List<Membership>
                {
                    new() {User = _currentUser, Role = GroupRole.Owner, DisplayName = "Test user"}
                }
            };
            
            _db.Groups.Add(group);
            await _db.SaveChangesAsync();

            return group;
        }

        private async Task<Group> CreateFilledTestGroup()
        {
            var game = new GroupGame();
            
            var group = await CreateEmptyTestGroup();
            group.Games = new List<GroupGame> {game};
            group.Events = new List<GroupEvent>
            {
                new()
                {
                    Game = game,
                    Slots = new List<GroupEventSlot>
                    {
                        new()
                    }
                }
            };

            await _db.SaveChangesAsync();

            return group;
        }
    }
}