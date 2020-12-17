using System.Collections.Generic;
using System.Linq;
using Database;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class MessageFilterModel : PageModel
    {
        public void OnGet()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = Database.Premium.IsPremium(auth.Guild.Id);

            List<MessageFilterRow> messageFilterRows = MessageFilter.GetRows(auth.Guild.Id);
            ViewData["messageFilterRows"] = messageFilterRows;

            List<RestTextChannel> channels = DiscordModule.GetTextChannelsAsync(auth.Guild).GetAwaiter().GetResult();
            ViewData["channels"] = channels;

            List<RestTextChannel> nonMessageFilterChannels = channels.Where(x => messageFilterRows.Count(y => y.ChannelId == x.Id) == 0).ToList();
            ViewData["nonMessageFilterChannels"] = nonMessageFilterChannels;
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
            int mode = int.Parse(HttpContext.Request.Form["mode"]);
            string complex = HttpContext.Request.Form["complex"].ToString();

            MessageFilterRow row = MessageFilter.GetRows(auth.Guild.Id, channelId).First();

            row.Mode = mode;
            row.Complex = EString.FromDecoded(complex);
            MessageFilter.SaveRow(row);

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
            RestTextChannel channel = auth.Guild.GetTextChannelAsync(channelId).GetAwaiter().GetResult();

            MessageFilterRow newRow = new MessageFilterRow
            {
                GuildId = auth.Guild.Id,
                ChannelId = channel.Id,
                Mode = 0,
                Complex = EString.FromDecoded("")
            };

            MessageFilter.SaveRow(newRow);
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

            MessageFilterRow deleteRow = MessageFilter.GetRows(auth.Guild.Id, channelId).First();

            MessageFilter.DeleteRow(deleteRow);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public static string GetIsSelected(int mode, MessageFilterRow row)
        {
            if (row.Mode == mode) return "selected";
            return "";
        }

        public static string GetIsComplexHidden(MessageFilterRow row)
        {
            if (row.Mode == 8) return "";
            return "hidden";
        }
    }
}
