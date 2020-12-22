using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class VoiceLinkModel : PageModel
    {
        public async Task OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = Database.Premium.IsPremium(auth.Guild.Id);

            VoiceLinkRow row = await VoiceLink.GetMetaRowAsync(auth.Guild.Id);

            List<RestVoiceChannel> voiceChannels = await DiscordModule.GetVoiceChannelsAsync(auth.Guild);
            List<RestVoiceChannel> excludedChannels = voiceChannels.Where(x => row.ExcludedChannels.Contains(x.Id)).OrderBy(x => x.Position).ToList();
            List<RestVoiceChannel> nonExcludedChannels = voiceChannels.Where(x => !row.ExcludedChannels.Contains(x.Id)).OrderBy(x => x.Position).ToList();

            ViewData["excludedChannels"] = excludedChannels;
            ViewData["nonExcludedChannels"] = nonExcludedChannels;
            ViewData["row"] = row;
        }

        public async Task OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            bool enabled = HttpContext.Request.Form["enabled"] == "on";
            bool deleteChannels = HttpContext.Request.Form["deleteChannels"] == "on";
            string prefix = HttpContext.Request.Form["prefix"].ToString();

            VoiceLinkRow row = await VoiceLink.GetMetaRowAsync(auth.Guild.Id);
            row.Enabled = enabled;
            row.DeleteChannels = deleteChannels;
            row.Prefix = EString.FromDecoded(prefix);
            await VoiceLink.SaveMetaRowAsync(row);

            HttpContext.Response.StatusCode = 200;
        }

        public async Task OnPostExclude()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            VoiceLinkRow metaRow = await VoiceLink.GetMetaRowAsync(auth.Guild.Id);
            if (!metaRow.ExcludedChannels.Contains(channelId))
            {
                metaRow.ExcludedChannels.Add(channelId);
            }
            await VoiceLink.SaveMetaRowAsync(metaRow);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public async Task OnPostRemove()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            VoiceLinkRow metaRow = await VoiceLink.GetMetaRowAsync(auth.Guild.Id);
            metaRow.ExcludedChannels.RemoveAll(x => x == channelId);
            await VoiceLink.SaveMetaRowAsync(metaRow);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public static string GetIsChecked(bool isChecked)
        {
            return isChecked ? "checked" : "";
        }
    }
}
