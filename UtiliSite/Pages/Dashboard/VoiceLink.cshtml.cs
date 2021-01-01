using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class VoiceLinkModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if(!auth.Authenticated) return auth.Action;

            VoiceLinkRow row = await VoiceLink.GetRowAsync(auth.Guild.Id);
            List<RestVoiceChannel> voiceChannels = await DiscordModule.GetVoiceChannelsAsync(auth.Guild);
            List<RestVoiceChannel> excludedChannels = voiceChannels.Where(x => row.ExcludedChannels.Contains(x.Id)).OrderBy(x => x.Position).ToList();
            List<RestVoiceChannel> nonExcludedChannels = voiceChannels.Where(x => !row.ExcludedChannels.Contains(x.Id)).OrderBy(x => x.Position).ToList();

            ViewData["row"] = row;
            ViewData["excludedChannels"] = excludedChannels;
            ViewData["nonExcludedChannels"] = nonExcludedChannels;

            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

                bool enabled = HttpContext.Request.Form["enabled"] == "on";
            bool deleteChannels = HttpContext.Request.Form["deleteChannels"] == "on";
            string prefix = HttpContext.Request.Form["prefix"].ToString();

            VoiceLinkRow row = await VoiceLink.GetRowAsync(auth.Guild.Id);
            row.Enabled = enabled;
            row.DeleteChannels = deleteChannels;
            row.Prefix = EString.FromDecoded(prefix);
            await VoiceLink.SaveRowAsync(row);

            return new OkResult();
        }

        public async Task<ActionResult> OnPostExclude()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            VoiceLinkRow metaRow = await VoiceLink.GetRowAsync(auth.Guild.Id);
            if (!metaRow.ExcludedChannels.Contains(channelId))
            {
                metaRow.ExcludedChannels.Add(channelId);
            }
            await VoiceLink.SaveRowAsync(metaRow);

            return new RedirectResult(Request.Path);
        }

        public async Task<ActionResult> OnPostRemove()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            VoiceLinkRow metaRow = await VoiceLink.GetRowAsync(auth.Guild.Id);
            metaRow.ExcludedChannels.RemoveAll(x => x == channelId);
            await VoiceLink.SaveRowAsync(metaRow);

            return new RedirectResult(Request.Path);
        }

        public static string GetIsChecked(bool isChecked)
        {
            return isChecked ? "checked" : "";
        }
    }
}
