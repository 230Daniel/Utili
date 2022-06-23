using Microsoft.AspNetCore.Authorization;

namespace Utili.Backend.Authorisation
{
    public class DiscordRequirement : IAuthorizationRequirement
    {
        public bool DiscordAuthenticated { get; set; }
    }
}
