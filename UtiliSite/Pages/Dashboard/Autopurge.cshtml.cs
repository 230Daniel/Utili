using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class AutopurgeModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if(!auth.Authenticated) return RedirectToPage("Index");

            List<AutopurgeRow> autopurgeRows = await Autopurge.GetRowsAsync(auth.Guild.Id);
            List<RestTextChannel> channels = await DiscordModule.GetTextChannelsAsync(auth.Guild);
            List<RestTextChannel> nonAutopurgeChannels = channels.Where(x => autopurgeRows.All(y => y.ChannelId != x.Id)).OrderBy(x => x.Position).ToList();

            ViewData["autopurgeRows"] = autopurgeRows;
            ViewData["channels"] = channels;
            ViewData["nonAutopurgeChannels"] = nonAutopurgeChannels;

            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if (!auth.Authenticated) return Forbid();

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);
            TimeSpan timespan = TimeSpan.Parse(HttpContext.Request.Form["timespan"]);
            int mode = int.Parse(HttpContext.Request.Form["mode"]);

            AutopurgeRow row = await Autopurge.GetRowAsync(auth.Guild.Id, channelId);
            row.Timespan = timespan;
            row.Mode = mode;
            await Autopurge.SaveRowAsync(row);

            return new OkResult();
        }

        public async Task<ActionResult> OnPostAdd()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if (!auth.Authenticated) return Forbid();

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);
            AutopurgeRow row = await Autopurge.GetRowAsync(auth.Guild.Id, channelId);
            try { await Autopurge.SaveRowAsync(row); }
            catch { }

            return new RedirectResult(Request.Path);
        }

        public async Task<ActionResult> OnPostRemove()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if (!auth.Authenticated) return Forbid();

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);
            AutopurgeRow row = await Autopurge.GetRowAsync(auth.Guild.Id, channelId);
            await Autopurge.DeleteRowAsync(row);

            return new RedirectResult(Request.Path);
        }

        public static string GetIsSelected(int mode, AutopurgeRow row)
        {
            if (row.Mode == mode) return "selected";
            return "";
        }
    }
}
