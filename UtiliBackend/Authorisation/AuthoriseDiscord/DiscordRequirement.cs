using Microsoft.AspNetCore.Authorization;

namespace UtiliBackend.Authorisation
{
    public class DiscordRequirement : IAuthorizationRequirement
    {
        public bool DiscordAuthenticated { get; set; }
    }
}
