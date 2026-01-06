using System;
using System.Threading.Tasks;
using GreenDonut;
using HotChocolate;
using HotChocolate.Types;
using LetsGame.Web.Data;
using LetsGame.Web.GraphQL.DataLoaders;
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
            var now = SystemClock.Instance.GetCurrentInstant();
            if (_membership.AvailableUntil == null || _membership.AvailableUntil < now) return null;
            
            return dateService.ConvertToUserLocalTime(_membership.AvailableUntil.Value);
        }

        public bool IsAvailableNow => _membership.IsAvailableNow();
        
        public async Task<GroupGraphType> GetGroup(IGroupByIdDataLoader loader)
        {
            var group = await loader.LoadRequiredAsync(_membership.GroupId);
            return new GroupGraphType(group);
        }

        public static (long groupId, string userId) ParseMembershipId(string id)
        {
            var rawId = ID.ToString<Membership>(id);
            var parts = rawId.Split("/");
            if (parts.Length != 2) throw new Exception("Invalid membership ID format");

            var groupId = long.Parse(parts[0]);
            var userId = parts[1];

            return (groupId, userId);
        }
    }
}