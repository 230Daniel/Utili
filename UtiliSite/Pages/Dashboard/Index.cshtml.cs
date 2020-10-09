using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Org.BouncyCastle.Asn1.Cmp;
using System.Text.Json;
using Discord.Rest;
using System.Data.Common;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Microsoft.AspNetCore.Http;

namespace UtiliSite.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            ViewData["guilds"] = new List<RestUserGuild>();

            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;

            if (auth.Guild == null)
                // We're displaying the guild select screen.
            {
                ViewData["avatarUrl"] = auth.Client.CurrentUser.GetAvatarUrl();
                ViewData["guilds"] = DiscordModule.GetManageableGuilds(auth.Client);
            }
            else
                // We're displaying the main page of the dashboard
            {
                ViewData["guildName"] = auth.Guild.Name;

                ViewData["prefix"] = Database.Data.Misc.GetPrefix(auth.Guild.Id);
                ViewData["nickname"] = DiscordModule.GetNickname(auth.Guild);
            }
        }

        public void OnPost()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);
            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            string prefix = HttpContext.Request.Form["prefix"];
            string nickname = HttpContext.Request.Form["nickname"];

            if (prefix != (string) ViewData["prefix"])
            {
                Database.Data.Misc.SetPrefix(auth.Guild.Id, prefix);
            }

            if (nickname != (string) ViewData["nickname"])
            {
                DiscordModule.SetNickname(auth.Guild, nickname);
            }

            HttpContext.Response.StatusCode = 200;
        }
    }

    public class IndexSettings
    {
        public string Prefix { get; set; }
    }
}
