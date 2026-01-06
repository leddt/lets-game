using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenDonut;
using HotChocolate;
using LetsGame.Web.Data;
using LetsGame.Web.GraphQL.DataLoaders;
using LetsGame.Web.Services;
using NodaTime;

namespace LetsGame.Web.GraphQL.Types
{
    public class SessionSlotGraphType : IdGraphType<GroupEventSlot>
    {
        private readonly GroupEventSlot _slot;

        public SessionSlotGraphType(GroupEventSlot slot)
        {
            _slot = slot;
        }

        protected override object GetId() => _slot.Id;

        public LocalDateTime GetProposedTime([Service] DateService dateService)
        {
            return dateService.ConvertToUserLocalTime(_slot.ProposedTime);
        }

        public async Task<IEnumerable<MembershipGraphType>> GetVoters(IMembershipsBySlotIdDataLoader loader)
        {
            var result = await loader.LoadAsync(_slot.Id) ?? [];
            return result.Select(x => new MembershipGraphType(x));
        }

        public async Task<ProposedSessionGraphType> GetSession(IGroupEventByIdDataLoader loader)
        {
            var result = await loader.LoadRequiredAsync(_slot.EventId);
            return new ProposedSessionGraphType(result);
        }
    }
}