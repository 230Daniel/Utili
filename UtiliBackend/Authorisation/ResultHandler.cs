using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace UtiliBackend.Authorisation
{
    public class ResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler;

        public ResultHandler()
        {
            _defaultHandler = new();
        }
        
        public async Task HandleAsync(
            RequestDelegate requestDelegate,
            HttpContext httpContext,
            AuthorizationPolicy authorisationPolicy,
            PolicyAuthorizationResult policyAuthorisationResult)
        {
            if (authorisationPolicy.Requirements.FirstOrDefault(x => x is DiscordGuildRequirement) is DiscordGuildRequirement guildRequirement)
            {
                if (!guildRequirement.DiscordAuthenticated)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return;
                }
                
                if (!guildRequirement.GuildManageable)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return;
                }
                
                if (!guildRequirement.GuildHasBot)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }
            }
            
            if (authorisationPolicy.Requirements.FirstOrDefault(x => x is DiscordRequirement) is DiscordRequirement {DiscordAuthenticated: false})
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }

            await _defaultHandler.HandleAsync(requestDelegate, httpContext, authorisationPolicy, policyAuthorisationResult);
        }
    }
}
