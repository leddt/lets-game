using System;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using LetsGame.Web.Data;
using LetsGame.Web.Services;
using NodaTime;

namespace LetsGame.Web.GraphQL.Types
{
    public class MembershipGraphType
    {
        private readonly Membership _membership;

        public MembershipGraphType(Membership membership)
        {
            _membership = membership;
        }

        [GraphQLType(typeof(IdType))]
        public string Id => ID.Typed<Membership>($"{_membership.GroupId}/{_membership.UserId}");
        public string DisplayName => _membership.DisplayName;
        public string UserId => _membership.UserId;
        public GroupRole Role => _membership.Role;

        public LocalDateTime? GetAvailableUntil([Service] DateService dateService)
        {
            if (_membership.AvailableUntilUtc == null || _membership.AvailableUntilUtc < DateTime.UtcNow) return null;
            
            return dateService.ConvertFromUtcToUserLocalTime(_membership.AvailableUntilUtc.Value);
        }

        public bool IsAvailableNow => _membership.IsAvailableNow();
        
        public async Task<GroupGraphType> GetGroup(IResolverContext context)
        {
            var group = await context.LoadGroup(_membership.GroupId);

            return group == null 
                ? null 
                : new GroupGraphType(group);
        }
    }
}