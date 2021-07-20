using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace UtiliBackend.Authorisation
{
    public class DiscordResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler;

        public DiscordResultHandler()
        {
            _defaultHandler = new();
        }
        
        public async Task HandleAsync(
            RequestDelegate requestDelegate,
            HttpContext httpContext,
            AuthorizationPolicy authorizationPolicy,
            PolicyAuthorizationResult policyAuthorizationResult)
        {
            if (!policyAuthorizationResult.Succeeded)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }

            await _defaultHandler.HandleAsync(requestDelegate, httpContext, authorizationPolicy, 
                policyAuthorizationResult);
        }
    }
}
