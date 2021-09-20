using Database.Entities;
using Discord.Rest;
using Microsoft.AspNetCore.Http;

namespace UtiliBackend.Extensions
{
    public static class HttpContextExtensions
    {
        public static DiscordRestClient GetDiscordClient(this HttpContext httpContext)
        {
            return (DiscordRestClient) httpContext.Items["DiscordClient"];
        }
        
        public static RestSelfUser GetDiscordUser(this HttpContext httpContext)
        {
            return httpContext.GetDiscordClient()?.CurrentUser;
        }
        
        public static RestGuild GetDiscordGuild(this HttpContext httpContext)
        {
            return (RestGuild) httpContext.Items["DiscordGuild"];
        }

        public static User GetUser(this HttpContext httpContext)
        {
            return (User) httpContext.Items["User"];
        }
    }
}
