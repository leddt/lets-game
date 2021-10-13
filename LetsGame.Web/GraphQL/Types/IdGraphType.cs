using HotChocolate;
using HotChocolate.Types;

namespace LetsGame.Web.GraphQL.Types
{
    public abstract class IdGraphType<T>
    {
        [GraphQLType(typeof(IdType))]
        public string Id => ID.Typed<T>(GetId());

        protected abstract object GetId();
    }
}