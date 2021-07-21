using Microsoft.AspNetCore.Authorization;

namespace UtiliBackend.Authorisation
{
    public class DiscordAuthoriseAttribute : AuthorizeAttribute
    {
        public DiscordAuthoriseAttribute()
        {
            Policy = "Discord";
        }
    }
}
