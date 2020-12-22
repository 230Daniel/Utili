using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class MessageFilterModel : PageModel
    {
        public async Task OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = Database.Premium.IsPremium(auth.Guild.Id);

            List<MessageFilterRow> messageFilterRows = await MessageFilter.GetRowsAsync(auth.Guild.Id);
            ViewData["messageFilterRows"] = messageFilterRows;

            List<RestTextChannel> channels = await DiscordModule.GetTextChannelsAsync(auth.Guild);
            ViewData["channels"] = channels;

            List<RestTextChannel> nonMessageFilterChannels = channels.Where(x => messageFilterRows.All(y => y.ChannelId != x.Id)).OrderBy(x => x.Position).ToList();
            ViewData["nonMessageFilterChannels"] = nonMessageFilterChannels;
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
            int mode = int.Parse(HttpContext.Request.Form["mode"]);
            string complex = HttpContext.Request.Form["complex"].ToString();

            MessageFilterRow row = await MessageFilter.GetRowAsync(auth.Guild.Id, channelId);
            row.Mode = mode;
            row.Complex = EString.FromDecoded(complex);
            await MessageFilter.SaveRowAsync(row);

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
            RestTextChannel channel = auth.Guild.GetTextChannelAsync(channelId).GetAwaiter().GetResult();

            MessageFilterRow row = await MessageFilter.GetRowAsync(auth.Guild.Id, channelId);
            await MessageFilter.SaveRowAsync(row);
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

            MessageFilterRow row = await MessageFilter.GetRowAsync(auth.Guild.Id, channelId);
            await MessageFilter.DeleteRowAsync(row);

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
