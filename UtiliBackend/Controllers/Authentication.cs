using System;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using static UtiliBackend.DiscordModule;

namespace UtiliBackend.Controllers
{
    public class Authentication : Controller
    {
        [HttpGet("auth")]
        public async Task<ActionResult> Auth()
        {
            AuthDetails auth = await GetAuthDetailsAsync(HttpContext);
            return auth.Authorised
                ? new JsonResult(new ShortAuthDetails(auth.User))
                : new JsonResult(new ShortAuthDetails());
        }

        [HttpGet("auth/signin")]
        public ActionResult SignInDiscord()
        {
            AuthenticationProperties authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = true,
                RedirectUri = $"{Main.Config.Frontend}/return"
            };

            return new ChallengeResult("Discord", authProperties);
        }

        [HttpPost("auth/signout")]
        public async Task<ActionResult> SignOutDiscord()
        {
            await HttpContext.SignOutAsync();
            return new OkResult();
        }

        public static async Task<AuthDetails> GetAuthDetailsAsync(HttpContext httpContext, ulong? guildId = null)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
                return new AuthDetails(new StatusCodeResult(401));

            ulong userId = ulong.Parse(httpContext.User.Claims.First(x => x.Type == "id").Value);
            string token = httpContext.GetTokenAsync("Discord", "access_token").GetAwaiter().GetResult();
            DiscordRestClient client = await GetClientAsync(userId, token);

            if (client is null)
                return new AuthDetails(new StatusCodeResult(401));

            AuthDetails auth = new AuthDetails(client, client.CurrentUser);

            if (guildId.HasValue)
            {
                RestGuild guild = await GetGuildAsync(guildId.Value);

                if (guild is null)
                {
                    if((await GetManageableGuildsAsync(client)).Any(x => x.Id == guildId)) return new AuthDetails(new StatusCodeResult(404));
                    return new AuthDetails(new StatusCodeResult(403));
                }
                if (await IsGuildManageableAsync(client.CurrentUser.Id, guildId.Value) || client.CurrentUser.Id == 218613903653863427)
                    auth.Guild = guild;
                else
                    return new AuthDetails(new StatusCodeResult(403));
            }
            return auth;
        }
    }

    public class AuthDetails
    {
        public bool Authorised { get; }
        public ActionResult Action { get; }
        public DiscordRestClient Client { get; }
        public RestSelfUser User { get; }
        public RestGuild Guild { get; set; }
        public UserRow UserRow { get; }

        public AuthDetails(DiscordRestClient client, RestSelfUser user)
        {
            Authorised = true;
            Client = client;
            User = user;

            UserRow = Users.GetRowAsync(user.Id).GetAwaiter().GetResult();
            if (UserRow.Email != user.Email || UserRow.LastVisit < DateTime.UtcNow - TimeSpan.FromMinutes(5))
            {
                UserRow.Email = user.Email;
                UserRow.LastVisit = DateTime.UtcNow;
                Users.SaveRowAsync(UserRow).GetAwaiter().GetResult();
            }
        }

        public AuthDetails(ActionResult action)
        {
            Authorised = false;
            Action = action;
        }
    }

    public class ShortAuthDetails
    {
        public bool Authenticated { get; }
        public string Username { get; }
        public string AvatarUrl { get; }

        public ShortAuthDetails()
        {
            Authenticated = false;
        }

        public ShortAuthDetails(RestSelfUser user)
        {
            Authenticated = true;
            Username = user.Username;
            AvatarUrl = user.GetAvatarUrl();
            if (string.IsNullOrEmpty(AvatarUrl)) AvatarUrl = user.GetDefaultAvatarUrl();
        }
    }
}
