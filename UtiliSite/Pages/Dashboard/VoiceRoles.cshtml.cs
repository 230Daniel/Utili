using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class VoiceRolesModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if(!auth.Authenticated) return auth.Action;

            List<VoiceRolesRow> rows = await VoiceRoles.GetRowsAsync(auth.Guild.Id);
            List<RestVoiceChannel> channels = await DiscordModule.GetVoiceChannelsAsync(auth.Guild);
            List<RestVoiceChannel> addedChannels = channels.Where(x => rows.Any(y => y.ChannelId == x.Id)).OrderBy(x => x.Position).ToList();
            List<RestVoiceChannel> nonAddedChannels = channels.Where(x => rows.All(y => y.ChannelId != x.Id)).OrderBy(x => x.Position).ToList();

            rows = rows.Where(x => channels.Any(y => y.Id == x.ChannelId)).ToList();

            ViewData["rows"] = rows;
            ViewData["addedChannels"] = addedChannels;
            ViewData["nonAddedChannels"] = nonAddedChannels;

            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

                ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);
            ulong roleId = ulong.Parse(HttpContext.Request.Form["role"]);

            VoiceRolesRow row = await VoiceRoles.GetRowAsync(auth.Guild.Id, channelId);
            row.RoleId = roleId;
            await VoiceRoles.SaveRowAsync(row);

            return new OkResult();
        }

        public async Task<ActionResult> OnPostAdd()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            VoiceRolesRow row = await VoiceRoles.GetRowAsync(auth.Guild.Id, channelId);
            try { await VoiceRoles.SaveRowAsync(row); }
            catch { }

            return new RedirectResult(Request.Path);
        }

        public async Task<ActionResult> OnPostRemove()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            VoiceRolesRow row = await VoiceRoles.GetRowAsync(auth.Guild.Id, channelId);
            await VoiceRoles.DeleteRowAsync(row);

            return new RedirectResult(Request.Path);
        }

        public static string GetIsSelected(ulong roleId, VoiceRolesRow row)
        {
            return row.RoleId == roleId ? "selected" : "";
        }
    }
}
