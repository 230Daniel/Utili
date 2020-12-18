using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using UtiliSite.Pages;
using static UtiliSite.DiscordModule;
using Database.Data;

namespace UtiliSite
{
    public static class Auth
    {
        public static async Task<AuthDetails> GetAuthDetailsAsync(HttpContext httpContext, string redirectUrl, string unauthorisedGuildUrl = "/dashboard")
        {
            AuthenticationProperties authProperties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl,
                AllowRefresh = true,
                IsPersistent = true
            };

            if (!httpContext.User.Identity.IsAuthenticated)
            {
                httpContext.ChallengeAsync("Discord", authProperties).GetAwaiter().GetResult();
                return new AuthDetails(false);
            }

            ulong userId = ulong.Parse(httpContext.User.Claims.First(x => x.Type == "id").Value);
            string token = httpContext.GetTokenAsync("Discord", "access_token").GetAwaiter().GetResult();
            DiscordRestClient client = await GetClientAsync(userId, token);

            if (client == null)
            {
                httpContext.ChallengeAsync("Discord", authProperties).GetAwaiter().GetResult();
                return new AuthDetails(false);
            }

            AuthDetails auth = new AuthDetails(true, client, client.CurrentUser, httpContext);

            if (httpContext.Request.RouteValues.TryGetValue("guild", out object guildValue))
            {
                if (ulong.TryParse(guildValue.ToString(), out ulong guildId))
                {
                    if (guildId > 0)
                    {
                        RestGuild guild = await GetGuildAsync(guildId);

                        if (guild == null)
                        {
                            string inviteUrl = "https://discord.com/api/oauth2/authorize?permissions=8&scope=bot&response_type=code" +
                                               $"&client_id={Main._config.DiscordClientId}" +
                                               $"&guild_id={guildId}" +
                                               $"&redirect_uri=https%3A%2F%2F{httpContext.Request.Host.Value}%2Freturn";

                            ReturnModel.SaveRedirect(userId,
                                $"https://{httpContext.Request.Host.Value}/dashboard/{guildId}/core");

                            httpContext.Response.Redirect(inviteUrl);
                            auth.Authenticated = false;
                            return auth;
                        }

                        if (await IsGuildManageableAsync(client.CurrentUser.Id, guild.Id))
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

        public static async Task<AuthDetails> GetOptionalGlobalAuthDetailsAsync(HttpContext httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return new AuthDetails(false);
            }

            ulong userId = ulong.Parse(httpContext.User.Claims.First(x => x.Type == "id").Value);
            string token = httpContext.GetTokenAsync("Discord", "access_token").GetAwaiter().GetResult();
            DiscordRestClient client = await GetClientAsync(userId, token);

            return client == null ? new AuthDetails(false) : new AuthDetails(true, client, client.CurrentUser, httpContext);
        }
    }

    public class AuthDetails
    {
        public bool Authenticated { get; set; }
        public DiscordRestClient Client { get; set; }
        public RestSelfUser User { get; set; }
        public RestGuild Guild { get; set; }

        private static List<string> _usedSessionIds = new List<string>();

        public AuthDetails(bool authenticated, DiscordRestClient client, RestSelfUser user, HttpContext httpContext)
        {
            Authenticated = authenticated;
            Client = client;
            User = user;

            _ = Task.Run(() =>
            {
                UserRow userRow = Users.GetRow(user.Id);
                DateTime previousVisit = userRow.LastVisit;
                userRow.Email = user.Email;
                userRow.LastVisit = DateTime.UtcNow;
                Users.SaveRow(userRow);

                if (previousVisit < DateTime.UtcNow - TimeSpan.FromHours(1)) Users.AddNewVisit(user.Id);
            });
        }

        public AuthDetails(bool authenticated)
        {
            Authenticated = authenticated;
        }
    }
}