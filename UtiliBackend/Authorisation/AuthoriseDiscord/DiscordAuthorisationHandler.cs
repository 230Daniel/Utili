using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using UtiliBackend.Services;

namespace UtiliBackend.Authorisation
{
    public class DiscordAuthorisationHandler : AuthorizationHandler<DiscordRequirement>
    {
        private readonly DiscordClientService _discordClientService;

        public DiscordAuthorisationHandler(DiscordClientService discordClientService)
        {
            _discordClientService = discordClientService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DiscordRequirement requirement)
        {
            var httpContext = (HttpContext) context.Resource;
            var client = await _discordClientService.GetClientAsync(httpContext);
                
            if (client is not null)
            {
                httpContext.Items["DiscordClient"] ??= client;
                
                if (client.Authorization.ExpiresAt > DateTimeOffset.Now)
                {
                    requirement.DiscordAuthenticated = true;
                    context.Succeed(requirement);
                }
            }
        }
    }
}
