
using HotChocolate.Resolvers;

namespace LetsGame.Web.Authorization
{
    public static class AuthExtensions
    {
        public static bool TryGetArgumentValue<T>(this IResolverContext ctx, string name, out T result)
        {
            try
            {
                result = ctx.ArgumentValue<T>(name);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        public static bool TryGetParent<T>(this IResolverContext ctx, out T result) where T : class
        {
            try
            {
                result = ctx.Parent<T>();
                return result != null;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}