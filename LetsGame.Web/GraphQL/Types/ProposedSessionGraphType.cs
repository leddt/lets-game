using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using LetsGame.Web.Data;
using LetsGame.Web.GraphQL.DataLoaders;
using LetsGame.Web.Services;
using NodaTime;

namespace LetsGame.Web.GraphQL.Types
{
    public class ProposedSessionGraphType : SessionGraphType
    {
        public ProposedSessionGraphType(GroupEvent groupEvent) : base(groupEvent)
        {
        }

        public async Task<IEnumerable<SessionSlotGraphType>> GetSlots(ISlotsByEventIdDataLoader loader)
        {
            var result = await loader.LoadAsync(GroupEvent.Id) ?? [];
            return result
                .Where(x => x.ProposedTime > SystemClock.Instance.GetCurrentInstant())
                .OrderBy(x => x.ProposedTime)
                .Select(x => new SessionSlotGraphType(x));
        }

        public async Task<IEnumerable<MembershipGraphType>> GetCantPlays(IMembershipsByCantPlayEventIdDataLoader loader)
        {
            var result = await loader.LoadAsync(GroupEvent.Id) ?? [];
            return result.Select(x => new MembershipGraphType(x));
        }

        public async Task<IEnumerable<MembershipGraphType>> GetMissingVotes(IMembershipsByGroupIdDataLoader membershipsByGroupIdLoader, IVoterIdsByEventIdDataLoader voterIdsLoader)
        {
            var allMembers = await membershipsByGroupIdLoader.LoadAsync(GroupEvent.GroupId) ?? [];
            var allVoterIds = await voterIdsLoader.LoadAsync(GroupEvent.Id) ?? [];

            return allMembers
                .Where(x => !allVoterIds.Contains(x.UserId))
                .Select(x => new MembershipGraphType(x));
        }

        public LocalDateTime? GetReminderSentAtTime([Service] DateService dateService)
        {
            if (GroupEvent.ReminderSentAt == null) return null;
            return dateService.ConvertToUserLocalTime(GroupEvent.ReminderSentAt.Value);
        }
    }
}