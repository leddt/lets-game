
using System.Diagnostics.CodeAnalysis;
using HotChocolate.Resolvers;

namespace LetsGame.Web.Authorization
{
    public static class AuthExtensions
    {
        extension(IResolverContext ctx)
        {
            public bool TryGetArgumentValue<T>(string name, [NotNullWhen(true)] out T? result)
            {
                try
                {
                    result = ctx.ArgumentValue<T>(name);
                    return result != null;
                }
                catch
                {
                    result = default;
                    return false;
                }
            }

            public bool TryGetParent<T>([NotNullWhen(true)] out T? result) where T : class
            {
                try
                {
                    result = ctx.Parent<T>();
                    return result != null!;
                }
                catch
                {
                    result = null;
                    return false;
                }
            }
        }
    }
}