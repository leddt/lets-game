using System.Security.Claims;

namespace LetsGame.Web.GraphQL.Types
{
    public class UserGraphType
    {
        private readonly ClaimsPrincipal _user;

        public UserGraphType(ClaimsPrincipal user)
        {
            _user = user;
        }

        public string Id => _user.FindFirstValue(ClaimTypes.NameIdentifier);
        public string Email => _user.FindFirstValue(ClaimTypes.Email);
    }
}