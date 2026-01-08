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
    public class UpcomingSessionGraphType : SessionGraphType
    {
        private readonly GroupEventSlot _slot;

        public UpcomingSessionGraphType(GroupEvent groupEvent) : base(groupEvent)
        {
            if (groupEvent.Slots == null) throw new InvalidOperationException("Slots not loaded");
            _slot = groupEvent.Slots.Single(x => x.ProposedTime == groupEvent.ChosenTime);
        }

        public LocalDateTime GetSessionTime([Service] DateService dateService)
        {
            return dateService.ConvertToUserLocalTime(_slot.ProposedTime);
        }

        public async Task<IEnumerable<MembershipGraphType>> GetParticipants(IMembershipsBySlotIdDataLoader loader)
        {
            var result = await loader.LoadAsync(_slot.Id) ?? [];
            return result.Select(x => new MembershipGraphType(x));
        } 
    }
}