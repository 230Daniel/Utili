using System.Threading.Tasks;
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

            ViewData["rows"] = await Database.Data.Premium.GetUserRowsAsync(auth.User.Id);
            ViewData["guilds"] = await DiscordModule.GetMutualGuildsAsync(auth.Client);
            ViewData["subscriptions"] = (await Subscriptions.GetRowsAsync(userId: auth.User.Id, onlyValid: true)).Count;
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

            PremiumRow row = await Database.Data.Premium.GetUserRowAsync(auth.User.Id, slotId);
            if (row == null)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            row.GuildId = guildId;
            await Database.Data.Premium.SaveRowAsync(row);

            HttpContext.Response.StatusCode = 200;
        }
    }
}
