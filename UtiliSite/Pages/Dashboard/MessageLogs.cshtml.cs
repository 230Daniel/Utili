using System;
using System.Collections.Generic;
using System.Linq;
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
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = Database.Data.Premium.IsGuildPremium(auth.Guild.Id);

            MessageLogsRow row = MessageLogs.GetRow(auth.Guild.Id);
            ViewData["row"] = row;

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

            ulong deletedChannelId = ulong.Parse(HttpContext.Request.Form["deletedChannel"]);
            ulong editedChannelId = ulong.Parse(HttpContext.Request.Form["editedChannel"]);

            MessageLogsRow row = MessageLogs.GetRow(auth.Guild.Id);
            row.DeletedChannelId = deletedChannelId;
            row.EditedChannelId = editedChannelId;
            MessageLogs.SaveRow(row);

            HttpContext.Response.StatusCode = 200;
        }

        public async Task OnPostAddExcludedChannel()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            MessageLogsRow row = MessageLogs.GetRow(auth.Guild.Id);
            if (!row.ExcludedChannels.Contains(channelId)) row.ExcludedChannels.Add(channelId);
            MessageLogs.SaveRow(row);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public async Task OnPostRemoveExcludedChannel()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            MessageLogsRow row = MessageLogs.GetRow(auth.Guild.Id);
            if (row.ExcludedChannels.Contains(channelId)) row.ExcludedChannels.Remove(channelId);
            MessageLogs.SaveRow(row);

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
