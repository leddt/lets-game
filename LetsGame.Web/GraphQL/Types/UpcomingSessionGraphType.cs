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
    public class UpcomingSessionGraphType : SessionGraphType
    {
        private readonly GroupEventSlot _slot;

        public UpcomingSessionGraphType(GroupEvent groupEvent) : base(groupEvent)
        {
            _slot = groupEvent.Slots.Single(x => x.ProposedTime == groupEvent.ChosenTime);
        }

        public LocalDateTime GetSessionTime([Service] DateService dateService)
        {
            return dateService.ConvertToUserLocalTime(_slot.ProposedTime);
        }

        public async Task<IEnumerable<MembershipGraphType>> GetParticipants(IResolverContext context)
        {
            var result = await context.LoadMembershipsBySlotId(_slot.Id);
            return result.Select(x => new MembershipGraphType(x));
        } 
    }
}