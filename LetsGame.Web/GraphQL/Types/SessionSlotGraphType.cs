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
            return dateService.ConvertFromUtcToUserLocalTime(_slot.ProposedDateAndTimeUtc);
        }

        public async Task<IEnumerable<MembershipGraphType>> GetVoters(IResolverContext context)
        {
            var result = await context.LoadMembershipsBySlotId(_slot.Id);
            return result.Select(x => new MembershipGraphType(x));
        }

        public async Task<ProposedSessionGraphType> GetSession(IResolverContext context)
        {
            var result = await context.LoadEvent(_slot.EventId);
            return new ProposedSessionGraphType(result);
        }
    }
}