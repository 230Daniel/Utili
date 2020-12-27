using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Database.Data;
using Microsoft.AspNetCore.Mvc;

namespace UtiliSite.Pages.Premium
{
    public class ManageModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            ViewData["auth"] = auth;
            if (!auth.Authenticated) return RedirectToPage("Index");

            ViewData["rows"] = await Database.Data.Premium.GetUserRowsAsync(auth.User.Id);
            ViewData["guilds"] = await DiscordModule.GetMutualGuildsAsync(auth.Client);
            ViewData["subscriptions"] = (await Subscriptions.GetRowsAsync(userId: auth.User.Id, onlyValid: true)).Count;

            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

            int slotId = int.Parse(HttpContext.Request.Form["slot"]);
            ulong guildId = ulong.Parse(HttpContext.Request.Form["guild"]);

            PremiumRow row = await Database.Data.Premium.GetUserRowAsync(auth.User.Id, slotId);
            if (row == null) return Forbid();

            row.GuildId = guildId;
            await Database.Data.Premium.SaveRowAsync(row);

            return new OkResult();
        }
    }
}
