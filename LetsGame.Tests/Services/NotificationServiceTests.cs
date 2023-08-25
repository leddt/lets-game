using System.Collections.Generic;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Moq;
using NodaTime;
using Xunit;

namespace LetsGame.Tests.Services
{
    public class NotificationServiceTests
    {
        private readonly ApplicationDbContext _db = TestUtils.GetInMemoryDbContext();
        private readonly Mock<IEmailSender> _emailSender;
        private readonly NotificationService _sut;
        
        public NotificationServiceTests()
        {
            var config = TestUtils.CreateConfiguration(new()
            {
                {"LocalTimezone", TestConfiguration.Timezone}
            });
            
            _emailSender = new Mock<IEmailSender>();
            _sut = new NotificationService(
                _db,
                new DateService(config, DateTimeZoneProviders.Bcl),
                _emailSender.Object,
                Mock.Of<IPushSender>(),
                Mock.Of<IHttpContextAccessor>());
        }

        [Fact]
        public async Task NotifyEventStartingSoon_SendsAnEmailToEachParticipant()
        {
            var ev = GetValidEvent();
            _db.GroupEvents.Add(ev);
            await _db.SaveChangesAsync();
            
            await _sut.NotifyEventStartingSoon(ev);
            
            _emailSender.Verify(x => x.SendEmailAsync("user1@example.com", It.IsAny<string>(), It.IsAny<string>()));
            _emailSender.Verify(x => x.SendEmailAsync("user2@example.com", It.IsAny<string>(), It.IsAny<string>()));
            _emailSender.Verify(x => x.SendEmailAsync("user4@example.com", It.IsAny<string>(), It.IsAny<string>()));
            _emailSender.Verify(x => x.SendEmailAsync("user5@example.com", It.IsAny<string>(), It.IsAny<string>()));
        }

        [Fact]
        public async Task NotifyEventStartingSoon_EmailContainsTheNamesOfTheOtherParticipants()
        {
            var ev = GetValidEvent();
            _db.GroupEvents.Add(ev);
            await _db.SaveChangesAsync();
            
            await _sut.NotifyEventStartingSoon(ev);
            
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
        public async Task NotifyEventStartingSoon_SubjectContainsTheGroupName()
        {
            var ev = GetValidEvent();
            _db.GroupEvents.Add(ev);
            await _db.SaveChangesAsync();
            
            await _sut.NotifyEventStartingSoon(ev);
            
            _emailSender.Verify(x => x.SendEmailAsync(
                "user1@example.com", 
                It.Is<string>(x => x.Contains("Test group")), 
                It.IsAny<string>()));
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