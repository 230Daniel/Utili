using System.Collections.Generic;
using System.Linq;
using Database;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class VoiceLinkModel : PageModel
    {
        public void OnGet()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;

            VoiceLinkRow metaRow = VoiceLink.GetMetaRow(auth.Guild.Id);

            List<RestVoiceChannel> voiceChannels = DiscordModule.GetVoiceChannelsAsync(auth.Guild).GetAwaiter().GetResult();
            List<RestVoiceChannel> excludedChannels = voiceChannels.Where(x => metaRow.ExcludedChannels.Contains(x.Id)).ToList();
            List<RestVoiceChannel> nonExcludedChannels = voiceChannels.Where(x => !metaRow.ExcludedChannels.Contains(x.Id)).ToList();

            ViewData["excludedChannels"] = excludedChannels;
            ViewData["nonExcludedChannels"] = nonExcludedChannels;
            ViewData["metaRow"] = metaRow;
        }

        public void OnPost()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            bool enabled = HttpContext.Request.Form["enabled"] == "on";
            bool deleteChannels = HttpContext.Request.Form["deleteChannels"] == "on";
            string prefix = HttpContext.Request.Form["prefix"].ToString();

            VoiceLinkRow row = VoiceLink.GetMetaRow(auth.Guild.Id);
            row.Enabled = enabled;
            row.DeleteChannels = deleteChannels;
            row.Prefix = EString.FromDecoded(prefix);
            VoiceLink.SaveMetaRow(row);

            HttpContext.Response.StatusCode = 200;
        }

        public void OnPostExclude()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channelId"]);

            VoiceLinkRow metaRow = VoiceLink.GetMetaRow(auth.Guild.Id);
            if (!metaRow.ExcludedChannels.Contains(channelId))
            {
                metaRow.ExcludedChannels.Add(channelId);
            }
            VoiceLink.SaveMetaRow(metaRow);

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

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channelId"]);

            VoiceLinkRow metaRow = VoiceLink.GetMetaRow(auth.Guild.Id);
            metaRow.ExcludedChannels.RemoveAll(x => x == channelId);
            VoiceLink.SaveMetaRow(metaRow);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public static string GetIsChecked(bool isChecked)
        {
            return isChecked ? "checked" : "";
        }
    }
}
