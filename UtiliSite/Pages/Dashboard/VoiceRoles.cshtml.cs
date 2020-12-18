using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class VoiceRolesModel : PageModel
    {
        public async Task OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = Database.Premium.IsPremium(auth.Guild.Id);

            List<RestVoiceChannel> voiceChannels = await DiscordModule.GetVoiceChannelsAsync(auth.Guild);
            List<VoiceRolesRow> rows = VoiceRoles.GetRows(auth.Guild.Id);

            List<RestVoiceChannel> addedChannels =
                voiceChannels.Where(x => rows.Any(y => y.ChannelId == x.Id)).OrderBy(x => x.Position).ToList();

            List<RestVoiceChannel> nonAddedChannels =
                voiceChannels.Where(x => rows.All(y => y.ChannelId != x.Id)).OrderBy(x => x.Position).ToList();

            ViewData["addedChannels"] = addedChannels;
            ViewData["nonAddedChannels"] = nonAddedChannels;
            ViewData["rows"] = rows;
        }

        public async Task OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);
            ulong roleId = ulong.Parse(HttpContext.Request.Form["role"]);

            VoiceRolesRow row = VoiceRoles.GetRows(auth.Guild.Id, channelId).First();
            row.RoleId = roleId;
            VoiceRoles.SaveRow(row);

            HttpContext.Response.StatusCode = 200;
        }

        public async Task OnPostAdd()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            VoiceRolesRow row = new VoiceRolesRow
            {
                GuildId = auth.Guild.Id,
                ChannelId = channelId,
                RoleId = 0
            };

            VoiceRoles.SaveRow(row);

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

            VoiceRolesRow row = VoiceRoles.GetRows(auth.Guild.Id, channelId).First();
            VoiceRoles.DeleteRow(row);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public static string GetIsSelected(ulong roleId, VoiceRolesRow row)
        {
            return row.RoleId == roleId ? "selected" : "";
        }
    }
}
