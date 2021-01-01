using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class MessageFilterModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if(!auth.Authenticated) return auth.Action;

            List<MessageFilterRow> messageFilterRows = await MessageFilter.GetRowsAsync(auth.Guild.Id);
            List<RestTextChannel> channels = await DiscordModule.GetTextChannelsAsync(auth.Guild);
            List<RestTextChannel> nonMessageFilterChannels = channels.Where(x => messageFilterRows.All(y => y.ChannelId != x.Id)).OrderBy(x => x.Position).ToList();

            ViewData["messageFilterRows"] = messageFilterRows;
            ViewData["channels"] = channels;
            ViewData["nonMessageFilterChannels"] = nonMessageFilterChannels;

            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);
            int mode = int.Parse(HttpContext.Request.Form["mode"]);
            string complex = HttpContext.Request.Form["complex"].ToString();

            MessageFilterRow row = await MessageFilter.GetRowAsync(auth.Guild.Id, channelId);
            row.Mode = mode;
            row.Complex = EString.FromDecoded(complex);
            await MessageFilter.SaveRowAsync(row);

            return new OkResult();
        }

        public async Task<ActionResult> OnPostAdd()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);
            MessageFilterRow row = await MessageFilter.GetRowAsync(auth.Guild.Id, channelId);
            try { await MessageFilter.SaveRowAsync(row); }
            catch { }

            return new RedirectResult(Request.Path);
        }

        public async Task<ActionResult> OnPostRemove()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if (!auth.Authenticated) return Forbid();

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            MessageFilterRow row = await MessageFilter.GetRowAsync(auth.Guild.Id, channelId);
            await MessageFilter.DeleteRowAsync(row);

            return new RedirectResult(Request.Path);
        }

        public static string GetIsSelected(int mode, MessageFilterRow row)
        {
            if (row.Mode == mode) return "selected";
            return "";
        }

        public static string GetIsComplexHidden(MessageFilterRow row)
        {
            if (row.Mode == 8) return "";
            return "hidden";
        }
    }
}
