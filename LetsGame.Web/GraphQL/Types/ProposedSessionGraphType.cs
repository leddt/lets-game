using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Resolvers;
using LetsGame.Web.Data;
using LetsGame.Web.Extensions;
using LetsGame.Web.Services;
using NodaTime;

namespace LetsGame.Web.GraphQL.Types
{
    public class ProposedSessionGraphType : SessionGraphType
    {
        public ProposedSessionGraphType(GroupEvent groupEvent) : base(groupEvent)
        {
        }

        public async Task<IEnumerable<SessionSlotGraphType>> GetSlots(IResolverContext context)
        {
            var result = await context.LoadSlotsByEventId(GroupEvent.Id);
            return result
                .Where(x => x.ProposedDateAndTimeUtc > DateTime.UtcNow)
                .OrderBy(x => x.ProposedDateAndTimeUtc)
                .Select(x => new SessionSlotGraphType(x));
        }

        public async Task<IEnumerable<MembershipGraphType>> GetCantPlays(IResolverContext context)
        {
            var result = await context.LoadMembershipsByCantPlayEventId(GroupEvent.Id);
            return result.Select(x => new MembershipGraphType(x));
        }

        public async Task<IEnumerable<MembershipGraphType>> GetMissingVotes(IResolverContext context)
        {
            var allMembers = await context.LoadMembershipsByGroupId(GroupEvent.GroupId);
            var allVoterIds = await context.LoadVoterIdsByEventId(GroupEvent.Id);

            return allMembers
                .Where(x => !allVoterIds.Contains(x.UserId))
                .Select(x => new MembershipGraphType(x));
        }

        public LocalDateTime? GetReminderSentAtTime([Service] DateService dateService)
        {
            if (GroupEvent.ReminderSentAtUtc == null) return null;
            return dateService.ConvertFromUtcToUserLocalTime(GroupEvent.ReminderSentAtUtc.Value);
        }
    }
}