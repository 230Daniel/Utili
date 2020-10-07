using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using static UtiliSite.DiscordModule;
using Discord.Rest;

namespace UtiliSite
{
    public class Auth
    {
        public static AuthDetails GetAuthDetails(HttpContext httpContext, string redirectUrl, string unauthorisedGuildUrl = "/dashboard", bool sendToAuthenticate = true)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                if (sendToAuthenticate)
                {
                    httpContext.ChallengeAsync("Discord", new AuthenticationProperties {RedirectUri = redirectUrl});
                }

                return new AuthDetails(false);
            }

            AuthDetails auth = new AuthDetails(
                true,
                httpContext.User.FindFirst(x => x.Type.Contains("identity/claims/nameidentifier")).Value,
                httpContext.User.Identity.Name,
                httpContext.User.FindFirst(x => x.Type.Contains("discord:user:discriminator")).Value,
                httpContext.User.FindFirst(x => x.Type.Contains("discord:avatar:url")).Value);

            if (httpContext.Request.RouteValues.TryGetValue("guild", out object guildValue))
            {
                if (ulong.TryParse(guildValue.ToString(), out ulong guildId))
                {
                    bool hasGuildPermission = false;

                    RestGuild guild = _client.GetGuildAsync(guildId).GetAwaiter().GetResult();
                    RestGuildUser user = guild?.GetUserAsync(auth.Id).GetAwaiter().GetResult();
                    if (user != null)
                    {
                        if (user.GuildPermissions.ManageGuild)
                        {
                            hasGuildPermission = true;
                            auth.Guild = guild;
                        }
                    }

                    if (!hasGuildPermission)
                    {
                        auth.Authenticated = false;
                        httpContext.Response.Redirect(unauthorisedGuildUrl);
                        return auth;
                    }
                }
                else
                {
                    auth.Authenticated = false;
                    httpContext.Response.Redirect(unauthorisedGuildUrl);
                    return auth;
                }
            }

            return auth;
        }
    }

    public class AuthDetails
    {
        public bool Authenticated { get; set; }
        public ulong Id { get; }
        public string Username { get; }
        public int Discriminator { get; }
        public string AvatarUrl { get; }
        public RestGuild Guild { get; set; }

        public AuthDetails(bool authenticated, string id, string username, string discriminator, string avatarUrl)
        {
            Authenticated = authenticated;
            Id = ulong.Parse(id);
            Username = username;
            Discriminator = int.Parse(discriminator);
            AvatarUrl = avatarUrl;
        }
        
        public AuthDetails(bool authenticated)
        {
            Authenticated = authenticated;
        }
    }
}