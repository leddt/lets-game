using System.Linq;
using HotChocolate.Resolvers;

namespace LetsGame.Web.Authorization
{
    public static class AuthExtensions
    {
        public static bool TryGetArgumentValue<T>(this IResolverContext ctx, string name, out T result)
        {
            var arg = ctx.Selection.SyntaxNode.Arguments.FirstOrDefault(x => x.Name.Value == name);
            
            if (arg == null)
            {
                result = default;
                return false;
            }

            var argValue = arg.Value.Value;
            if (argValue is T value)
            {
                result = value;
                return true;
            }

            result = default;
            return false;
        }

        public static bool TryGetParent<T>(this IResolverContext ctx, out T result) where T : class
        {
            result = ctx.Parent<T>();
            return result != null;
        }
    }
}