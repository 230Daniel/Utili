using System.Linq;
using Discord.Rest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using UtiliSite.Pages;
using static UtiliSite.DiscordModule;

namespace UtiliSite
{
    public class Auth
    {
        public static AuthDetails GetAuthDetails(HttpContext httpContext, string redirectUrl, string unauthorisedGuildUrl = "/dashboard")
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                AuthenticationProperties authProperties = new AuthenticationProperties
                {
                    RedirectUri = redirectUrl
                };

                httpContext.ChallengeAsync("Discord", authProperties).GetAwaiter().GetResult();

                return new AuthDetails(false);
            }

            ulong userId = ulong.Parse(httpContext.User.Claims.First(x => x.Type == "id").Value);
            string token = httpContext.GetTokenAsync("Discord", "access_token").GetAwaiter().GetResult();

            DiscordRestClient client = GetClient(userId, token);

            AuthDetails auth = new AuthDetails(true, client, client.CurrentUser);

            if (httpContext.Request.RouteValues.TryGetValue("guild", out object guildValue))
            {
                if (ulong.TryParse(guildValue.ToString(), out ulong guildId))
                {
                    if (guildId > 0)
                    {
                        RestGuild guild = GetGuildAsync(guildId).GetAwaiter().GetResult();

                        if (guild == null)
                        {
                            if (IsGuildManageable(client, guildId))
                            {
                                string inviteUrl = "https://discord.com/api/oauth2/authorize?permissions=8&scope=bot&response_type=code" +
                                                   $"&client_id={Main._config.DiscordClientId}" +
                                                   $"&guild_id={guildId}" +
                                                   $"&redirect_uri=https%3A%2F%2F{httpContext.Request.Host.Value}%2Freturn";

                                ReturnModel.SaveRedirect(userId,
                                    $"https://{httpContext.Request.Host.Value}/dashboard/{guildId}");

                                httpContext.Response.Redirect(inviteUrl);
                                auth.Authenticated = false;
                                return auth;
                            }
                            
                            auth.Authenticated = false;
                            httpContext.Response.Redirect(unauthorisedGuildUrl);
                            return auth;
                        }

                        if (IsGuildManageable(client, guild.Id))
                        {
                            auth.Guild = guild;
                        }
                        else
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
        public DiscordRestClient Client { get; set; }
        public RestSelfUser User { get; set; }
        public RestGuild Guild { get; set; }

        public AuthDetails(bool authenticated, DiscordRestClient client, RestSelfUser user)
        {
            Authenticated = authenticated;
            Client = client;
            User = user;
        }

        public AuthDetails(bool authenticated)
        {
            Authenticated = authenticated;
        }
    }
}