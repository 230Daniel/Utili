using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Database.Data;
using Discord.Rest;

namespace UtiliSite.Pages.Dashboard
{
    public class VoiceLinkModel : PageModel
    {
        public void OnGet()
        {
            ViewData["authorised"] = false;
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;
            ViewData["authorised"] = true;

            ViewData["guild"] = auth.Guild;
            ViewData["Title"] = $"{auth.Guild.Name} - ";

            List<RestVoiceChannel> voiceChannels = DiscordModule.GetVoiceChannelsAsync(auth.Guild).GetAwaiter().GetResult();
            List<VoiceLinkChannelRow> rows = VoiceLink.GetChannelRows(auth.Guild.Id);

            List<RestVoiceChannel> excludedChannels = new List<RestVoiceChannel>();

            foreach (RestVoiceChannel voiceChannel in voiceChannels)
            {
                List<VoiceLinkChannelRow> applicableRows = rows.Where(x => x.VoiceChannelId == voiceChannel.Id).ToList();
                if (applicableRows.Count > 0 && applicableRows.First().Excluded)
                {
                    excludedChannels.Add(voiceChannel);
                }
            }

            List<RestVoiceChannel> nonExcludedChannels =
                voiceChannels.Where(x => !excludedChannels.Select(y => y.Id).Contains(x.Id)).ToList();

            ViewData["excludedChannels"] = excludedChannels;
            ViewData["nonExcludedChannels"] = nonExcludedChannels;
            ViewData["metaRow"] = VoiceLink.GetMetaRow(auth.Guild.Id);
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

            VoiceLinkChannelRow channelRow = VoiceLink.GetChannelRow(auth.Guild.Id, channelId);
            channelRow.Excluded = true;
            VoiceLink.SaveChannelRow(channelRow);

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

            VoiceLinkChannelRow channelRow = VoiceLink.GetChannelRow(auth.Guild.Id, channelId);
            channelRow.Excluded = false;
            VoiceLink.SaveChannelRow(channelRow);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public static string GetIsChecked(bool isChecked)
        {
            if (isChecked) return "checked";
            return "";
        }
    }
}
