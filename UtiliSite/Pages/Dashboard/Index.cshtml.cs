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

                
            }
        }
    }
}
