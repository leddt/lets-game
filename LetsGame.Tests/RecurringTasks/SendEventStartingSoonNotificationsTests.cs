using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.RecurringTasks;
using LetsGame.Web.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Moq;
using Xunit;

namespace LetsGame.Tests.RecurringTasks
{
    public class SendEventStartingSoonNotificationsTests
    {
        private readonly ApplicationDbContext _db = TestUtils.GetInMemoryDbContext();
        private readonly SendEventStartingSoonNotifications _sut;
        private readonly Mock<IEmailSender> _emailSender;

        public SendEventStartingSoonNotificationsTests()
        {
            var config = TestUtils.CreateConfiguration(new()
            {
                {"LocalTimezone", TestConfiguration.Timezone}
            });

            _emailSender = new Mock<IEmailSender>();
            _sut = new SendEventStartingSoonNotifications(_db, new BatchMemberMailer(Mock.Of<ICurrentUserAccessor>(), null, _emailSender.Object), new DateService(config));
        }
        
        [Fact]
        public async Task SendsAnEmailToEachParticipant()
        {
            var ev = GetValidEvent();
            await _sut.SendEventNotifications(ev);
            
            _emailSender.Verify(x => x.SendEmailAsync("user1@example.com", It.IsAny<string>(), It.IsAny<string>()));
            _emailSender.Verify(x => x.SendEmailAsync("user2@example.com", It.IsAny<string>(), It.IsAny<string>()));
            _emailSender.Verify(x => x.SendEmailAsync("user4@example.com", It.IsAny<string>(), It.IsAny<string>()));
            _emailSender.Verify(x => x.SendEmailAsync("user5@example.com", It.IsAny<string>(), It.IsAny<string>()));
        }

        [Fact]
        public async Task TheEmailContainsTheNamesOfTheOtherParticipants()
        {
            var ev = GetValidEvent();
            await _sut.SendEventNotifications(ev);
            
            _emailSender.Verify(x => x.SendEmailAsync(
                "user1@example.com", 
                It.IsAny<string>(), 
                It.Is<string>(s => s.Contains("Fifth user, Fourth user and Second user"))));
            
            _emailSender.Verify(x => x.SendEmailAsync(
                "user2@example.com", 
                It.IsAny<string>(), 
                It.Is<string>(s => s.Contains("Fifth user, First user and Fourth user"))));
        }

        [Fact]
        public async Task TheSubjectContainsTheGroupName()
        {
            var ev = GetValidEvent();
            await _sut.SendEventNotifications(ev);
            
            _emailSender.Verify(x => x.SendEmailAsync(
                "user1@example.com", 
                It.Is<string>(x => x.Contains("Test group")), 
                It.IsAny<string>()));
        }

        [Fact]
        public async Task TheJobFindsTheProperEvents()
        {
            var validEvent = GetValidEvent();
            validEvent.Group.Name = "Valid";
            
            var invalidEvent1 = GetValidEvent();
            invalidEvent1.Group.Name = "Invalid";
            invalidEvent1.ChosenDateAndTimeUtc = DateTime.UtcNow + TimeSpan.FromHours(3);
            
            var invalidEvent2 = GetValidEvent();
            invalidEvent1.Group.Name = "Invalid";
            invalidEvent2.StartingSoonNotificationSentAtUtc = DateTime.UtcNow;
            
            _db.GroupEvents.Add(validEvent);
            _db.GroupEvents.Add(invalidEvent1);
            _db.GroupEvents.Add(invalidEvent2);
            await _db.SaveChangesAsync();
            _db.ChangeTracker.Clear();
            
            await _sut.Run();

            _emailSender.Verify(
                x => x.SendEmailAsync(
                    It.IsAny<string>(),
                    It.Is<string>(s => s.Contains("Valid")),
                    It.IsAny<string>()),
                Times.AtLeast(1));
            
            _emailSender.Verify(
                x => x.SendEmailAsync(
                    It.IsAny<string>(),
                    It.Is<string>(s => s.Contains("Invalid")),
                    It.IsAny<string>()),
                Times.Never);
        }

        private GroupEvent GetValidEvent()
        {
            var chosenTime = DateTime.UtcNow.AddHours(1);

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
                ChosenDateAndTimeUtc = chosenTime,
                Slots = new List<GroupEventSlot>
                {
                    new()
                    {
                        ProposedDateAndTimeUtc = chosenTime,
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