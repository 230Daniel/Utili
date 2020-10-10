using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class CoreModel : PageModel
    {
        public void OnGet()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;

            ViewData["guildName"] = auth.Guild.Name;

            ViewData["prefix"] = Database.Data.Misc.GetPrefix(auth.Guild.Id);
            ViewData["nickname"] = DiscordModule.GetNickname(auth.Guild);
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
}
