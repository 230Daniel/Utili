using System;
using System.Collections.Generic;
using System.Linq;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class ChannelMirroringModel : PageModel
    {
        public void OnGet()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;

            ViewData["guild"] = auth.Guild;
            ViewData["user"] = auth.User;

            List<ChannelMirroringRow> rows = ChannelMirroring.GetRows(auth.Guild.Id);
            ViewData["rows"] = rows;

            List<RestTextChannel> channels = DiscordModule.GetTextChannelsAsync(auth.Guild).GetAwaiter().GetResult();
            ViewData["channels"] = channels;
        }

        public void OnPost()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            long id = int.Parse(HttpContext.Request.Form["rowId"]);
            ulong toChannelId = ulong.Parse(HttpContext.Request.Form["toChannel"]);

            ChannelMirroringRow row = ChannelMirroring.GetRows(id: id, guildId: auth.Guild.Id).First();

            if (row.FromChannelId == toChannelId)
            {
                HttpContext.Response.StatusCode = 400;
                return;
            }

            row.ToChannelId = toChannelId;

            ChannelMirroring.SaveRow(row);

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

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channelId"]);
            RestTextChannel channel = auth.Guild.GetTextChannelAsync(channelId).GetAwaiter().GetResult();

            ChannelMirroringRow newRow = new ChannelMirroringRow(auth.Guild.Id, channelId);

            ChannelMirroring.SaveRow(newRow);
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

            long deleteId = long.Parse(HttpContext.Request.Form["rowId"]);

            ChannelMirroringRow deleteRow = ChannelMirroring.GetRows(id: deleteId, guildId: auth.Guild.Id).First();

            ChannelMirroring.DeleteRow(deleteRow);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }
    }
}
