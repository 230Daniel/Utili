using Microsoft.AspNetCore.Authorization;

namespace UtiliBackend.Authorisation
{
    public class DiscordGuildAuthoriseAttribute : AuthorizeAttribute
    {
        public DiscordGuildAuthoriseAttribute()
        {
            Policy = "DiscordGuild";
        }
    }
}
