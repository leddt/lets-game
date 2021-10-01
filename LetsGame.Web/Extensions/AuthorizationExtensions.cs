using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace LetsGame.Web.Extensions
{
    public static class AuthorizationExtensions
    {
        public static async Task EnsureAuthorized(
            this IAuthorizationService authorizationService, 
            ClaimsPrincipal user, 
            object resource, 
            string policyName)
        {
            var authResult = await authorizationService.AuthorizeAsync(user, resource, policyName);
            if (!authResult.Succeeded) throw new Exception("Unauthorized");
        }
    }
}