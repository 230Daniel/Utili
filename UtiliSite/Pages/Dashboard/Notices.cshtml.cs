using System;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Database;
using Discord.Rest;

namespace UtiliSite.Pages.Dashboard
{
    public class NoticesModel : PageModel
    {
        public async Task OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;

            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = Premium.IsPremium(auth.Guild.Id);
            ViewData["rows"] = await Notices.GetRowsAsync(auth.Guild.Id);
            ViewData["channels"] = await DiscordModule.GetTextChannelsAsync(auth.Guild);
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

            NoticesRow row = await Notices.GetRowAsync(auth.Guild.Id, channelId);

            row.Enabled = HttpContext.Request.Form["enabled"] == "on";
            row.Delay = TimeSpan.Parse(HttpContext.Request.Form["delay"]);
            row.Title = EString.FromDecoded(HttpContext.Request.Form["title"]);
            row.Footer = EString.FromDecoded(HttpContext.Request.Form["footer"]);
            row.Content = EString.FromDecoded(HttpContext.Request.Form["content"]);
            row.Text = EString.FromDecoded(HttpContext.Request.Form["text"]);
            row.Image = EString.FromDecoded(HttpContext.Request.Form["image"]);
            row.Thumbnail = EString.FromDecoded(HttpContext.Request.Form["thumbnail"]);
            row.Icon = EString.FromDecoded(HttpContext.Request.Form["icon"]);
            row.Colour = new Discord.Color(uint.Parse(HttpContext.Request.Form["colour"].ToString().Replace("#", ""), System.Globalization.NumberStyles.HexNumber));

            await Notices.SaveRowAsync(row);

            MiscRow miscRow = new MiscRow(auth.Guild.Id, "RequiresNoticeUpdate", row.ChannelId.ToString());
            try { await Misc.SaveRowAsync(miscRow); } catch { }

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

            NoticesRow row = new NoticesRow(auth.Guild.Id, channel.Id);

            await Notices.SaveRowAsync(row);
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
            NoticesRow row = await Notices.GetRowAsync(auth.Guild.Id, channelId);
            await Notices.DeleteRowAsync(row);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public static string GetIsIdSelected(ulong idOne, ulong idTwo)
        {
            return idTwo == idOne ? "selected" : "";
        }

        public static string GetIsBoolSelected(bool boolOne, bool boolTwo)
        {
            return boolTwo == boolOne ? "selected" : "";
        }
    }
}
