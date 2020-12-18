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
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = Database.Premium.IsPremium(auth.Guild.Id);

            List<AutopurgeRow> autopurgeRows = Autopurge.GetRows(auth.Guild.Id);
            ViewData["autopurgeRows"] = autopurgeRows;

            List<RestTextChannel> channels = await DiscordModule.GetTextChannelsAsync(auth.Guild);
            ViewData["channels"] = channels;

            List<RestTextChannel> nonAutopurgeChannels = channels.Where(x => autopurgeRows.All(y => y.ChannelId != x.Id)).OrderBy(x => x.Position).ToList();
            ViewData["nonAutopurgeChannels"] = nonAutopurgeChannels;

            
        }

        public async Task OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);
            TimeSpan timespan = TimeSpan.Parse(HttpContext.Request.Form["timespan"]);
            int mode = int.Parse(HttpContext.Request.Form["mode"]);

            AutopurgeRow row = Autopurge.GetRows(auth.Guild.Id, channelId).First();

            row.Timespan = timespan;
            row.Mode = mode;

            Autopurge.SaveRow(row);

            HttpContext.Response.StatusCode = 200;
        }

        public async Task OnPostAdd()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);
            RestTextChannel channel = auth.Guild.GetTextChannelAsync(channelId).GetAwaiter().GetResult();

            AutopurgeRow newRow = new AutopurgeRow
            {
                GuildId = auth.Guild.Id,
                ChannelId = channel.Id,
                Timespan = TimeSpan.FromMinutes(15),
                Mode = 2
            };

            Autopurge.SaveRow(newRow);
            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public async Task OnPostRemove()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            AutopurgeRow deleteRow = Autopurge.GetRows(auth.Guild.Id, channelId).First();

            Autopurge.DeleteRow(deleteRow);

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
