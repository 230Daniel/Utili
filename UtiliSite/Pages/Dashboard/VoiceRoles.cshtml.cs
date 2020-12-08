using System.Collections.Generic;
using System.Linq;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class VoiceRolesModel : PageModel
    {
        public void OnGet()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;

            List<RestVoiceChannel> voiceChannels = DiscordModule.GetVoiceChannelsAsync(auth.Guild).GetAwaiter().GetResult().OrderBy(x => x.Position).ToList();
            List<VoiceRolesRow> rows = VoiceRoles.GetRows(auth.Guild.Id);

            List<RestVoiceChannel> addedChannels =
                voiceChannels.Where(x => rows.Select(y => y.ChannelId).Contains(x.Id)).ToList();

            List<RestVoiceChannel> nonAddedChannels =
                voiceChannels.Where(x => !addedChannels.Select(y => y.Id).Contains(x.Id)).ToList();

            ViewData["addedChannels"] = addedChannels;
            ViewData["nonAddedChannels"] = nonAddedChannels;
            ViewData["rows"] = rows;
        }

        public void OnPost()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);

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

        public void OnPostAdd()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);

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

        public void OnPostRemove()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);

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
