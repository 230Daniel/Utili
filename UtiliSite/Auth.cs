using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using UtiliSite.Pages;
using static UtiliSite.DiscordModule;
using Database.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
namespace UtiliSite
{
    public static class Auth
    {
        public static async Task<AuthDetails> GetAuthDetailsAsync(PageModel model)
        {
            return await GetAuthDetailsAsync(model.HttpContext, model.ViewData);
        }

        public static async Task<AuthDetails> GetAuthDetailsAsync(Controller controller)
        {
            return await GetAuthDetailsAsync(controller.HttpContext, controller.ViewData);
        }

        private static async Task<AuthDetails> GetAuthDetailsAsync(HttpContext httpContext, ViewDataDictionary viewData)
        {
            AuthenticationProperties authProperties = new AuthenticationProperties
            {
                RedirectUri = httpContext.Request.Path,
                AllowRefresh = true,
                IsPersistent = true
            };
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return new AuthDetails(new ChallengeResult("Discord", authProperties));
            }

            ulong userId = ulong.Parse(httpContext.User.Claims.First(x => x.Type == "id").Value);
            string token = httpContext.GetTokenAsync("Discord", "access_token").GetAwaiter().GetResult();
            DiscordRestClient client = await GetClientAsync(userId, token);

            if (client is null)
            {
                return new AuthDetails(new ChallengeResult("Discord", authProperties));
            }

            AuthDetails auth = new AuthDetails(client, client.CurrentUser);
            viewData["user"] = auth.User;

            if (httpContext.Request.RouteValues.TryGetValue("guild", out object guildValue))
            {
                if (ulong.TryParse(guildValue.ToString(), out ulong guildId) && guildId > 0)
                {
                    RestGuild guild = await GetGuildAsync(guildId);

                    if (guild is null && (await GetManageableGuildsAsync(client)).Any(x => x.Id == guildId))
                    {
                        string inviteUrl = "https://discord.com/api/oauth2/authorize?permissions=8&scope=bot&response_type=code" +
                                           $"&client_id={Main.Config.DiscordClientId}" +
                                           $"&guild_id={guildId}" +
                                           $"&redirect_uri=https%3A%2F%2F{httpContext.Request.Host.Value}%2Freturn";

                        ReturnModel.SaveRedirect(userId,
                            $"https://{httpContext.Request.Host.Value}/dashboard/{guildId}/core");

                        return new AuthDetails(new RedirectResult(inviteUrl));
                    }

                    if (guild is not null && (await IsGuildManageableAsync(client.CurrentUser.Id, guildId) || client.CurrentUser.Id == 218613903653863427))
                    {
                        auth.Guild = guild;
                        viewData["guild"] = guild;
                        viewData["premium"] = await Premium.IsGuildPremiumAsync(auth.Guild.Id);
                    }
                    else
                    {
                        return new AuthDetails(new RedirectResult($"https://{httpContext.Request.Host.Value}/dashboard"));
                    }
                }
                else
                {
                    return new AuthDetails(new RedirectResult($"https://{httpContext.Request.Host.Value}/dashboard"));
                }
            }

            viewData["auth"] = auth;
            return auth;
        }

        public static async Task<AuthDetails> GetOptionalAuthDetailsAsync(PageModel model)
        {
            return await GetOptionalAuthDetailsAsync(model.HttpContext, model.ViewData);
        }

        public static async Task<AuthDetails> GetOptionalAuthDetailsAsync(Controller controller)
        {
            return await GetOptionalAuthDetailsAsync(controller.HttpContext, controller.ViewData);
        }

        private static async Task<AuthDetails> GetOptionalAuthDetailsAsync(HttpContext httpContext, ViewDataDictionary viewData)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return new AuthDetails(null);
            }

            ulong userId = ulong.Parse(httpContext.User.Claims.First(x => x.Type == "id").Value);
            string token = httpContext.GetTokenAsync("Discord", "access_token").GetAwaiter().GetResult();
            DiscordRestClient client = await GetClientAsync(userId, token);

            AuthDetails auth = client is null ? new AuthDetails(null) : new AuthDetails(client, client.CurrentUser);
            viewData["auth"] = auth;
            viewData["user"] = auth.User;
            return auth;
        }
    }

    public class AuthDetails
    {
        public bool Authenticated { get; set; }
        public ActionResult Action { get; set; }
        public DiscordRestClient Client { get; set; }
        public RestSelfUser User { get; set; }
        public RestGuild Guild { get; set; }
        public UserRow UserRow { get; set; }

        public AuthDetails(DiscordRestClient client, RestSelfUser user)
        {
            Authenticated = true;
            Client = client;
            User = user;

            UserRow = Users.GetRowAsync(user.Id).GetAwaiter().GetResult();
            DateTime previousVisit = UserRow.LastVisit;
            UserRow.Email = user.Email;
            
            if (previousVisit < DateTime.UtcNow - TimeSpan.FromMinutes(30))
            {
                UserRow.LastVisit = DateTime.UtcNow;
                Users.SaveRowAsync(UserRow).GetAwaiter().GetResult();
                //_ = Users.AddNewVisitAsync(user.Id);
            }
        }

        public AuthDetails(ActionResult action)
        {
            Authenticated = false;
            Action = action;
        }
    }
}