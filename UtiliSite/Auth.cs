using System;
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
        public static async Task<AuthDetails> GetAuthDetailsAsync(HttpContext httpContext)
        {
            AuthenticationProperties authProperties = new AuthenticationProperties
            {
                RedirectUri = httpContext.Request.Path,
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

            AuthDetails auth = new AuthDetails(true, client, client.CurrentUser);

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
                                               $"&client_id={Main.Config.DiscordClientId}" +
                                               $"&guild_id={guildId}" +
                                               $"&redirect_uri=https%3A%2F%2F{httpContext.Request.Host.Value}%2Freturn";

                            ReturnModel.SaveRedirect(userId,
                                $"https://{httpContext.Request.Host.Value}/dashboard/{guildId}/core");

                            httpContext.Response.Redirect(inviteUrl);
                            auth.Authenticated = false;
                            return auth;
                        }

                        if (await IsGuildManageableAsync(client.CurrentUser.Id, guild.Id) || client.CurrentUser.Id == 218613903653863427)
                        {
                            auth.Guild = guild;
                        }
                        else
                        {
                            auth.Authenticated = false;
                            httpContext.Response.Redirect("/dashboard");
                            return auth;
                        }
                    }
                    else
                    {
                        auth.Authenticated = false;
                        httpContext.Response.Redirect("/dashboard");
                        return auth;
                    }
                }
                else
                {
                    auth.Authenticated = false;
                    httpContext.Response.Redirect("/dashboard");
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

            return client == null ? new AuthDetails(false) : new AuthDetails(true, client, client.CurrentUser);
        }
    }

    public class AuthDetails
    {
        public bool Authenticated { get; set; }
        public DiscordRestClient Client { get; set; }
        public RestSelfUser User { get; set; }
        public RestGuild Guild { get; set; }
        public UserRow UserRow { get; set; }

        public AuthDetails(bool authenticated, DiscordRestClient client, RestSelfUser user)
        {
            Authenticated = authenticated;
            Client = client;
            User = user;

            UserRow = Users.GetRow(user.Id);
            DateTime previousVisit = UserRow.LastVisit;
            UserRow.Email = user.Email;
                
            if (previousVisit < DateTime.UtcNow - TimeSpan.FromMinutes(30))
            {
                UserRow.LastVisit = DateTime.UtcNow;
                Users.SaveRow(UserRow);
                Users.AddNewVisit(user.Id);
            }
        }

        public AuthDetails(bool authenticated)
        {
            Authenticated = authenticated;
        }
    }
}