using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Database.Data;
using Discord.Rest;

namespace UtiliSite.Pages.Dashboard
{
    public class AutopurgeModel : PageModel
    {
        public void OnGet()
        {
            ViewData["authorised"] = false;
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;
            ViewData["authorised"] = true;

            ViewData["guild"] = auth.Guild;
            ViewData["Title"] = $"{auth.Guild.Name} - ";

            List<AutopurgeRow> autopurgeRows = Autopurge.GetRows(auth.Guild.Id);
            ViewData["autopurgeRows"] = autopurgeRows;
        }

        public void OnPost()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            if (!string.IsNullOrEmpty(HttpContext.Request.Form["addChannel"]))
            {
                ulong channelId = ulong.Parse(HttpContext.Request.Form["addChannel"]);
                RestTextChannel channel = auth.Guild.GetTextChannelAsync(channelId).GetAwaiter().GetResult();

                AutopurgeRow newRow = new AutopurgeRow()
                {
                    GuildId = auth.Guild.Id,
                    ChannelId = channel.Id,
                    Timespan = TimeSpan.FromMinutes(15),
                    Mode = 2
                };

                Autopurge.SaveRow(newRow);
                HttpContext.Response.StatusCode = 200;
                HttpContext.Response.Redirect(HttpContext.Request.Path);
                return;
            }

            if (!string.IsNullOrEmpty(HttpContext.Request.Form["removeChannel"]))
            {
                int deleteId = int.Parse(HttpContext.Request.Form["removeChannel"]);

                AutopurgeRow deleteRow = Autopurge.GetRows(id: deleteId, guildId: auth.Guild.Id).First();

                Autopurge.DeleteRow(deleteRow);

                HttpContext.Response.StatusCode = 200;
                HttpContext.Response.Redirect(HttpContext.Request.Path);
                return;
            }

            int id = int.Parse(HttpContext.Request.Form["rowId"]);
            TimeSpan timespan = TimeSpan.Parse(HttpContext.Request.Form["timespan"]);
            int mode = int.Parse(HttpContext.Request.Form["mode"]);

            AutopurgeRow row = Autopurge.GetRows(id: id, guildId: auth.Guild.Id).First();

            if (row.GuildId != auth.Guild.Id)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            row.Timespan = timespan;
            row.Mode = mode;

            Autopurge.SaveRow(row);

            HttpContext.Response.StatusCode = 200;
        }

        public static string GetIsSelected(int mode, AutopurgeRow row)
        {
            if (row.Mode == mode) return "selected";
            return "";
        }
    }
}
