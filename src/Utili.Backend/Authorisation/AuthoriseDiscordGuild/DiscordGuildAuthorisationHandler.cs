using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Utili.Backend.Services;

namespace Utili.Backend.Authorisation;

public class DiscordGuildAuthorisationHandler : AuthorizationHandler<DiscordGuildRequirement>
{
    private readonly DiscordUserGuildsService _discordUserGuildsService;
    private readonly DiscordRestService _discordRestService;

    public DiscordGuildAuthorisationHandler(DiscordUserGuildsService discordUserGuildsService, DiscordRestService discordRestService)
    {
        _discordUserGuildsService = discordUserGuildsService;
        _discordRestService = discordRestService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DiscordGuildRequirement requirement)
    {
        var identity = context.User.Identities.FirstOrDefault(x => x.AuthenticationType == "Discord");

        if (identity is not null && identity.IsAuthenticated)
        {
            var httpContext = (HttpContext)context.Resource;
            var managedGuilds = await _discordUserGuildsService.GetManagedGuildsAsync(httpContext);

            var guildId = ulong.Parse((string)httpContext.Request.RouteValues["GuildId"] ?? "0");

            if (managedGuilds.Guilds.Any(x => x.Id == guildId))
            {
                requirement.GuildManageable = true;

                var guild = await _discordRestService.GetGuildAsync(guildId);
                if (guild is not null)
                {
                    requirement.GuildHasBot = true;
                    context.Succeed(requirement);

                    httpContext.Items["DiscordGuild"] ??= guild;
                }
            }
        }
    }
}