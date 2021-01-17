using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace LetsGame.Web.Services
{
    public class HttpContextCurrentUserAccessor : ICurrentUserAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextCurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal CurrentUser => _httpContextAccessor.HttpContext?.User;
    }
}