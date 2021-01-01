using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class ChannelMirroringModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if(!auth.Authenticated) return auth.Action;

            List<ChannelMirroringRow> rows = await ChannelMirroring.GetRowsAsync(auth.Guild.Id);
            List<RestTextChannel> channels = await DiscordModule.GetTextChannelsAsync(auth.Guild);

            ViewData["rows"] = rows;
            ViewData["channels"] = channels;

            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

                ulong fromChannelId = ulong.Parse(HttpContext.Request.Form["fromChannel"]);
            ulong toChannelId = ulong.Parse(HttpContext.Request.Form["toChannel"]);

            ChannelMirroringRow row = await ChannelMirroring.GetRowAsync(auth.Guild.Id, fromChannelId);
            row.ToChannelId = toChannelId;
            try { await ChannelMirroring.SaveRowAsync(row); }
            catch { }

            return new OkResult();
        }

        public async Task<ActionResult> OnPostAdd()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channelId"]);
            ChannelMirroringRow row = await ChannelMirroring.GetRowAsync(auth.Guild.Id, channelId);

            try { await ChannelMirroring.SaveRowAsync(row); }
            catch { }

            return new RedirectResult(Request.Path);
        }

        public async Task<ActionResult> OnPostRemove()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

            ulong fromChannelId = ulong.Parse(HttpContext.Request.Form["fromChannel"]);
            ChannelMirroringRow row = await ChannelMirroring.GetRowAsync(auth.Guild.Id, fromChannelId);
            await ChannelMirroring.DeleteRowAsync(row);

            return new RedirectResult(Request.Path);
        }
    }
}
