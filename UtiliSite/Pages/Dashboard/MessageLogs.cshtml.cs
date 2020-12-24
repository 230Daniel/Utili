using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class MessageLogsModel : PageModel
    {
        public async Task OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if(!auth.Authenticated) return;

            MessageLogsRow row = await MessageLogs.GetRowAsync(auth.Guild.Id);
            List<RestTextChannel> channels = await DiscordModule.GetTextChannelsAsync(auth.Guild);

            ViewData["row"] = row;
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

            ulong deletedChannelId = ulong.Parse(HttpContext.Request.Form["deletedChannel"]);
            ulong editedChannelId = ulong.Parse(HttpContext.Request.Form["editedChannel"]);

            MessageLogsRow row = await MessageLogs.GetRowAsync(auth.Guild.Id);
            row.DeletedChannelId = deletedChannelId;
            row.EditedChannelId = editedChannelId;
            await MessageLogs.SaveRowAsync(row);

            HttpContext.Response.StatusCode = 200;
        }

        public async Task OnPostExclude()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            MessageLogsRow row = await MessageLogs.GetRowAsync(auth.Guild.Id);
            if (!row.ExcludedChannels.Contains(channelId)) row.ExcludedChannels.Add(channelId);
            try { await MessageLogs.SaveRowAsync(row); }
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

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            MessageLogsRow row = await MessageLogs.GetRowAsync(auth.Guild.Id);
            if (row.ExcludedChannels.Contains(channelId)) row.ExcludedChannels.Remove(channelId);
            await MessageLogs.SaveRowAsync(row);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
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
