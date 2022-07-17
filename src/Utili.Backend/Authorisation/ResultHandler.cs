using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace Utili.Backend.Authorisation;

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
        if (authorisationPolicy.Requirements.Any(x => x is DiscordRequirement { DiscordAuthenticated: false }))
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        if (authorisationPolicy.Requirements.FirstOrDefault(x => x is DiscordGuildRequirement) is DiscordGuildRequirement guildRequirement)
        {
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

        await _defaultHandler.HandleAsync(requestDelegate, httpContext, authorisationPolicy, policyAuthorisationResult);
    }
}