#nullable enable

using HotChocolate;
using HotChocolate.Types;

namespace LetsGame.Web.GraphQL.Types
{
    public class UpdateSessionInput : ISessionIdInput
    {
        [GraphQLType(typeof(IdType))] public string SessionId { get; set; } = null!;
        
        public string? Details { get; set; }
    }
}