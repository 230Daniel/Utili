using Microsoft.AspNetCore.Authorization;

namespace Utili.Backend.Authorisation;

public class DiscordGuildAuthoriseAttribute : AuthorizeAttribute
{
    public DiscordGuildAuthoriseAttribute()
    {
        Policy = "DiscordGuild";
    }
}