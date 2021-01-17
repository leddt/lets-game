using System.Security.Claims;

namespace LetsGame.Web.Services
{
    public interface ICurrentUserAccessor
    {
        public ClaimsPrincipal CurrentUser { get; }
    }
}