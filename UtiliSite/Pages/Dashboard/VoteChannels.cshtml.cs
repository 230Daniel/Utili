using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class VoteChannelsModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if(!auth.Authenticated) return RedirectToPage("Index");

            List<VoteChannelsRow> rows = await VoteChannels.GetRowsAsync(auth.Guild.Id);
            List<RestTextChannel> channels = await DiscordModule.GetTextChannelsAsync(auth.Guild);
            List<RestTextChannel> nonVoteChannels = channels.Where(x => rows.All(y => y.ChannelId != x.Id)).OrderBy(x => x.Position).ToList();

            ViewData["rows"] = rows;
            ViewData["channels"] = channels;
            ViewData["nonVoteChannels"] = nonVoteChannels;

            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);
            int mode = int.Parse(HttpContext.Request.Form["mode"]);

            VoteChannelsRow row = await VoteChannels.GetRowAsync(auth.Guild.Id, channelId);
            row.Mode = mode;
            await VoteChannels.SaveRowAsync(row);

            return new OkResult();
        }

        public async Task<ActionResult> OnPostAdd()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);
            VoteChannelsRow newRow = await VoteChannels.GetRowAsync(auth.Guild.Id, channelId);
            try { await VoteChannels.SaveRowAsync(newRow); }
            catch { }

            return new RedirectResult(Request.Path);
        }

        public async Task<ActionResult> OnPostRemove()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

                ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            VoteChannelsRow row = await VoteChannels.GetRowAsync(auth.Guild.Id, channelId);
            await VoteChannels.DeleteRowAsync(row);

            return new RedirectResult(Request.Path);
        }

        public static string GetIsSelected(int mode, VoteChannelsRow row)
        {
            if (row.Mode == mode) return "selected";
            return "";
        }
    }
}
