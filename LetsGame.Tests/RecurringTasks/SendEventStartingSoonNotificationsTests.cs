using System.Collections.Generic;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.RecurringTasks;
using LetsGame.Web.Services;
using Moq;
using NodaTime;
using Xunit;

namespace LetsGame.Tests.RecurringTasks
{
    public class SendEventStartingSoonNotificationsTests
    {
        private readonly ApplicationDbContext _db = TestUtils.GetInMemoryDbContext();
        private readonly Mock<INotificationService> _notificationService;
        private readonly SendEventStartingSoonNotifications _sut;

        public SendEventStartingSoonNotificationsTests()
        {
            _notificationService = new Mock<INotificationService>();
            _sut = new SendEventStartingSoonNotifications(_db, _notificationService.Object);
        }

        [Fact]
        public async Task TheJobFindsTheProperEvents()
        {
            var validEvent = GetValidEvent();
            validEvent.Group.Name = "Valid";
            
            var invalidEvent1 = GetValidEvent();
            invalidEvent1.Group.Name = "Invalid";
            invalidEvent1.ChosenTime = SystemClock.Instance.GetCurrentInstant() + Duration.FromHours(3);
            
            var invalidEvent2 = GetValidEvent();
            invalidEvent1.Group.Name = "Invalid";
            invalidEvent2.StartingSoonNotificationSentAt = SystemClock.Instance.GetCurrentInstant();
            
            _db.GroupEvents.Add(validEvent);
            _db.GroupEvents.Add(invalidEvent1);
            _db.GroupEvents.Add(invalidEvent2);
            await _db.SaveChangesAsync();
            _db.ChangeTracker.Clear();
            
            await _sut.Run();

            _notificationService.Verify(x => x.NotifyEventStartingSoon(It.Is<GroupEvent>(x => x.Id == validEvent.Id)));
        }

        private GroupEvent GetValidEvent()
        {
            var chosenTime = SystemClock.Instance.GetCurrentInstant() + Duration.FromHours(1);

            var user1 = new AppUser {Email = "user1@example.com"};
            var user2 = new AppUser {Email = "user2@example.com"};
            var user3 = new AppUser {Email = "user3@example.com"};
            var user4 = new AppUser {Email = "user4@example.com"};
            var user5 = new AppUser {Email = "user5@example.com"};

            return new GroupEvent
            {
                Group = new Group
                {
                    Name = "Test group",
                    Memberships = new List<Membership>
                    {
                        new() {User = user1, UserId = user1.Id, DisplayName = "First user"},
                        new() {User = user2, UserId = user2.Id, DisplayName = "Second user"},
                        new() {User = user3, UserId = user3.Id, DisplayName = "Third user"},
                        new() {User = user4, UserId = user4.Id, DisplayName = "Fourth user"},
                        new() {User = user5, UserId = user5.Id, DisplayName = "Fifth user"}
                    }
                },
                Details = "This is a test event",
                Game = new GroupGame {Name = "Test game"},
                ChosenTime = chosenTime,
                Slots = new List<GroupEventSlot>
                {
                    new()
                    {
                        ProposedTime = chosenTime,
                        Votes = new List<GroupEventSlotVote>
                        {
                            new() {VoterId = user1.Id},
                            new() {VoterId = user2.Id},
                            new() {VoterId = user4.Id},
                            new() {VoterId = user5.Id}
                        }
                    }
                }
            };
        }
    }
}