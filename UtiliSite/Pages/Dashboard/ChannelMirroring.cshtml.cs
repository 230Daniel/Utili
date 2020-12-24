using System.Collections.Generic;
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
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if(!auth.Authenticated) return;

            List<ChannelMirroringRow> rows = await ChannelMirroring.GetRowsAsync(auth.Guild.Id);
            List<RestTextChannel> channels = await DiscordModule.GetTextChannelsAsync(auth.Guild);

            ViewData["rows"] = rows;
            ViewData["channels"] = channels;
        }

        public async Task OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong fromChannelId = ulong.Parse(HttpContext.Request.Form["fromChannel"]);
            ulong toChannelId = ulong.Parse(HttpContext.Request.Form["toChannel"]);

            ChannelMirroringRow row = await ChannelMirroring.GetRowAsync(auth.Guild.Id, fromChannelId);
            row.ToChannelId = toChannelId;
            try { await ChannelMirroring.SaveRowAsync(row); }
            catch { }

            HttpContext.Response.StatusCode = 200;
        }

        public async Task OnPostAdd()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channelId"]);
            ChannelMirroringRow row = await ChannelMirroring.GetRowAsync(auth.Guild.Id, channelId);

            try { await ChannelMirroring.SaveRowAsync(row); }
            catch { }
            
            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public async Task OnPostRemove()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong fromChannelId = ulong.Parse(HttpContext.Request.Form["fromChannel"]);
            ChannelMirroringRow row = await ChannelMirroring.GetRowAsync(auth.Guild.Id, fromChannelId);
            await ChannelMirroring.DeleteRowAsync(row);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }
    }
}
