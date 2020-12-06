using System;
using System.Collections.Generic;
using System.Linq;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class AutopurgeModel : PageModel
    {
        public void OnGet()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;

            List<AutopurgeRow> autopurgeRows = Autopurge.GetRows(auth.Guild.Id);
            ViewData["autopurgeRows"] = autopurgeRows;

            List<RestTextChannel> channels = DiscordModule.GetTextChannelsAsync(auth.Guild).GetAwaiter().GetResult();
            ViewData["channels"] = channels;

            List<RestTextChannel> nonAutopurgeChannels = channels.Where(x => autopurgeRows.Count(y => y.ChannelId == x.Id) == 0).ToList();
            ViewData["nonAutopurgeChannels"] = nonAutopurgeChannels;
        }

        public void OnPost()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);

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

        public void OnPostAdd()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);

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

        public void OnPostRemove()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);

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
