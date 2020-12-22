using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Database.Data;

namespace UtiliSite.Pages.Premium
{
    public class ManageModel : PageModel
    {
        public async Task OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);
            ViewData["auth"] = auth;
            if (!auth.Authenticated) return;

            ViewData["rows"] = Database.Data.Premium.GetUserRows(auth.User.Id);
            ViewData["guilds"] = await DiscordModule.GetMutualGuildsAsync(auth.Client);
            ViewData["subscriptions"] = Subscriptions.GetRows(userId: auth.User.Id).Count;
        }

        public async Task OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            int slotId = int.Parse(HttpContext.Request.Form["slot"]);
            ulong guildId = ulong.Parse(HttpContext.Request.Form["guild"]);

            PremiumRow row = Database.Data.Premium.GetUserRow(auth.User.Id, slotId);
            if (row == null)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            row.GuildId = guildId;
            Database.Data.Premium.SaveRow(row);

            HttpContext.Response.StatusCode = 200;
        }
    }
}
