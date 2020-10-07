using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using static UtiliSite.DiscordModule;
using Discord.Rest;
using Microsoft.AspNetCore.Identity;

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

            string token = httpContext.GetTokenAsync("Discord", "access_token").GetAwaiter().GetResult();

            DiscordRestClient client = Login(token);

            AuthDetails auth = new AuthDetails(true, client, token);

            if (httpContext.Request.RouteValues.TryGetValue("guild", out object guildValue))
            {
                if (ulong.TryParse(guildValue.ToString(), out ulong guildId))
                {
                    bool hasGuildPermission = false;

                    if (guildId > 0)
                    {
                        RestGuild guild = null;
                        try
                        {
                            guild = _client.GetGuildAsync(guildId).GetAwaiter().GetResult();
                        }
                        catch
                        {
                        }

                        if (guild == null)
                        {
                            string inviteUrl =
                                $"https://discord.com/api/oauth2/authorize?client_id={Main._config.DiscordClientId}&guild_id={guildId}&redirect_uri=https%3A%2F%2Flocalhost%3A44347%2Fdashboard&permissions=8&scope=bot";
                            httpContext.Response.WriteAsync($@"<script type='text/javascript' language='javascript'>history.go(-1);window.open('{inviteUrl}','_blank').focus();</script>").GetAwaiter().GetResult();
                            auth.Authenticated = false;
                            return auth;
                        }
                        else
                        {
                            RestGuildUser guildUser =
                                guild.GetUserAsync(auth.Client.CurrentUser.Id).GetAwaiter().GetResult();
                            if (guildUser != null)
                            {
                                if (guildUser.GuildPermissions.ManageGuild)
                                {
                                    hasGuildPermission = true;
                                    auth.Guild = guild;
                                }
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
            }

            return auth;
        }
    }

    public class AuthDetails
    {
        public bool Authenticated { get; set; }
        public DiscordRestClient Client { get; set; }
        public string Token { get; }
        public RestGuild Guild { get; set; }

        public AuthDetails(bool authenticated, DiscordRestClient client, string token)
        {
            Authenticated = authenticated;
            Client = client;
            Token = token;
        }

        public AuthDetails(bool authenticated)
        {
            Authenticated = authenticated;
        }
    }
}