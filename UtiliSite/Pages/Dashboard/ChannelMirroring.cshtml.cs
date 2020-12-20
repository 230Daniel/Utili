using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class ChannelMirroringModel : PageModel
    {
        public async Task OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = Database.Premium.IsPremium(auth.Guild.Id);

            List<ChannelMirroringRow> rows = ChannelMirroring.GetRows(auth.Guild.Id);
            ViewData["rows"] = rows;

            List<RestTextChannel> channels = await DiscordModule.GetTextChannelsAsync(auth.Guild);
            ViewData["channels"] = channels;
        }

        public async Task OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);

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

        public async Task OnPostAdd()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);

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

        public async Task OnPostRemove()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);

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
