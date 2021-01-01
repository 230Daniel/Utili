using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class ReputationModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if(!auth.Authenticated) return auth.Action;

            ReputationRow row = await Reputation.GetRowAsync(auth.Guild.Id);

            ViewData["row"] = row;
            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

            ReputationRow row = await Reputation.GetRowAsync(auth.Guild.Id);
            (IEmote, int) emote = row.Emotes.First(x => x.Item1.ToString() == HttpContext.Request.Form["emote"]);
            int value = int.Parse(HttpContext.Request.Form["value"]);

            row.Emotes.Remove(emote);
            row.Emotes.Add((emote.Item1, value));
            await Reputation.SaveRowAsync(row);

            return new OkResult();
        }

        public async Task<ActionResult> OnPostRemove()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return auth.Action;

            ReputationRow row = await Reputation.GetRowAsync(auth.Guild.Id);
            (IEmote, int) emote = row.Emotes.First(x => x.Item1.ToString() == HttpContext.Request.Form["emote"]);
            row.Emotes.Remove(emote);

            try { await Reputation.SaveRowAsync(row); }
            catch { }

            return new RedirectResult(Request.Path);
        }
    }
}
