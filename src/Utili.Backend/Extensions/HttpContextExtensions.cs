using Utili.Database.Entities;
using Disqord;
using Microsoft.AspNetCore.Http;
using Utili.Backend.Services;

namespace Utili.Backend.Extensions
{
    public static class HttpContextExtensions
    {
        public static DiscordClientService.BearerClient GetDiscordClient(this HttpContext httpContext)
        {
            return (DiscordClientService.BearerClient)httpContext.Items["DiscordClient"];
        }

        public static ICurrentUser GetDiscordUser(this HttpContext httpContext)
        {
            return httpContext.GetDiscordClient()?.User;
        }

        public static IGuild GetDiscordGuild(this HttpContext httpContext)
        {
            return (IGuild)httpContext.Items["DiscordGuild"];
        }

        public static User GetUser(this HttpContext httpContext)
        {
            return (User)httpContext.Items["User"];
        }
    }
}
