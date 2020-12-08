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
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = Database.Premium.IsPremium(auth.Guild.Id);

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

            ulong fromChannelId = ulong.Parse(HttpContext.Request.Form["fromChannel"]);
            ulong toChannelId = ulong.Parse(HttpContext.Request.Form["toChannel"]);

            ChannelMirroringRow row = ChannelMirroring.GetRows(auth.Guild.Id, fromChannelId).First();

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

            ulong fromChannelId = ulong.Parse(HttpContext.Request.Form["fromChannel"]);

            ChannelMirroringRow deleteRow = ChannelMirroring.GetRows(auth.Guild.Id, fromChannelId).First();

            ChannelMirroring.DeleteRow(deleteRow);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }
    }
}
