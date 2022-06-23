using Microsoft.AspNetCore.Authorization;

namespace Utili.Backend.Authorisation
{
    public class DiscordGuildRequirement : IAuthorizationRequirement
    {
        public bool GuildManageable { get; set; }
        public bool GuildHasBot { get; set; }
    }
}
