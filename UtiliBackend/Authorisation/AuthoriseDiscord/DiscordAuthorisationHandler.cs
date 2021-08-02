using System.Linq;
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
            var identity = context.User.Identities.FirstOrDefault(x => x.AuthenticationType == "Discord");
            
            if (identity is not null && identity.IsAuthenticated)
            {
                var httpContext = (HttpContext) context.Resource;
                var client = await _discordClientService.GetClientAsync(httpContext);
                httpContext.Items["DiscordClient"] ??= client;
                
                if (client is not null)
                {
                    requirement.DiscordAuthenticated = true;
                    context.Succeed(requirement);
                }
            }
        }
    }
}
