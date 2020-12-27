using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class MessageLogsModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if(!auth.Authenticated) return RedirectToPage("Index");

            MessageLogsRow row = await MessageLogs.GetRowAsync(auth.Guild.Id);
            List<RestTextChannel> channels = await DiscordModule.GetTextChannelsAsync(auth.Guild);

            ViewData["row"] = row;
            ViewData["channels"] = channels;

            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

            ulong deletedChannelId = ulong.Parse(HttpContext.Request.Form["deletedChannel"]);
            ulong editedChannelId = ulong.Parse(HttpContext.Request.Form["editedChannel"]);

            MessageLogsRow row = await MessageLogs.GetRowAsync(auth.Guild.Id);
            row.DeletedChannelId = deletedChannelId;
            row.EditedChannelId = editedChannelId;
            await MessageLogs.SaveRowAsync(row);

            return new OkResult();
        }

        public async Task<ActionResult> OnPostExclude()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if (!auth.Authenticated) return Forbid();

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            MessageLogsRow row = await MessageLogs.GetRowAsync(auth.Guild.Id);
            if (!row.ExcludedChannels.Contains(channelId)) row.ExcludedChannels.Add(channelId);
            try { await MessageLogs.SaveRowAsync(row); }
            catch { }

            return new RedirectResult(Request.Path);
        }

        public async Task<ActionResult> OnPostRemove()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if (!auth.Authenticated) return Forbid();

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            MessageLogsRow row = await MessageLogs.GetRowAsync(auth.Guild.Id);
            if (row.ExcludedChannels.Contains(channelId)) row.ExcludedChannels.Remove(channelId);
            await MessageLogs.SaveRowAsync(row);

            return new RedirectResult(Request.Path);
        }

        public static string GetIsChannelSelected(ulong roleOne, ulong roleTwo)
        {
            return roleTwo == roleOne ? "selected" : "";
        }

        public static string GetIsBoolSelected(bool boolOne, bool boolTwo)
        {
            return boolTwo == boolOne ? "selected" : "";
        }
    }
}
