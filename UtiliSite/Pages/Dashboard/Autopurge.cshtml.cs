using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class AutopurgeModel : PageModel
    {
        public async Task OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = await Database.Data.Premium.IsGuildPremiumAsync(auth.Guild.Id);

            List<AutopurgeRow> autopurgeRows = await Autopurge.GetRowsAsync(auth.Guild.Id);
            ViewData["autopurgeRows"] = autopurgeRows;

            List<RestTextChannel> channels = await DiscordModule.GetTextChannelsAsync(auth.Guild);
            ViewData["channels"] = channels;

            List<RestTextChannel> nonAutopurgeChannels = channels.Where(x => autopurgeRows.All(y => y.ChannelId != x.Id)).OrderBy(x => x.Position).ToList();
            ViewData["nonAutopurgeChannels"] = nonAutopurgeChannels;
        }

        public async Task OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);
            TimeSpan timespan = TimeSpan.Parse(HttpContext.Request.Form["timespan"]);
            int mode = int.Parse(HttpContext.Request.Form["mode"]);

            AutopurgeRow row = await Autopurge.GetRowAsync(auth.Guild.Id, channelId);
            row.Timespan = timespan;
            row.Mode = mode;
            await Autopurge.SaveRowAsync(row);

            HttpContext.Response.StatusCode = 200;
        }

        public async Task OnPostAdd()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);
            AutopurgeRow row = await Autopurge.GetRowAsync(auth.Guild.Id, channelId);
            try { await Autopurge.SaveRowAsync(row); }
            catch { }

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public async Task OnPostRemove()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);
            AutopurgeRow row = await Autopurge.GetRowAsync(auth.Guild.Id, channelId);
            await Autopurge.DeleteRowAsync(row);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public static string GetIsSelected(int mode, AutopurgeRow row)
        {
            if (row.Mode == mode) return "selected";
            return "";
        }
    }
}
