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
            return httpContext.GetDiscordClient().CurrentUser;
        }
    }
}
