using Microsoft.AspNetCore.Authorization;

namespace UtiliBackend.Authorisation
{
    public class DiscordGuildRequirement : IAuthorizationRequirement
    {
        public bool DiscordAuthenticated { get; set; }
        public bool GuildManageable { get; set; }
        public bool GuildHasBot { get; set; }
    }
}
