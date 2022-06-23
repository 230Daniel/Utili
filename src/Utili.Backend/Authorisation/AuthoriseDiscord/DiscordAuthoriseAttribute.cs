using Microsoft.AspNetCore.Authorization;

namespace Utili.Backend.Authorisation
{
    public class DiscordAuthoriseAttribute : AuthorizeAttribute
    {
        public DiscordAuthoriseAttribute()
        {
            Policy = "Discord";
        }
    }
}
