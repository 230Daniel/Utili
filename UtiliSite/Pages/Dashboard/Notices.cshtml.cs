using System;
using System.Linq;
using Database.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Database;
using Discord.Rest;

namespace UtiliSite.Pages.Dashboard
{
    public class NoticesModel : PageModel
    {
        public void OnGet()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;

            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = Premium.IsPremium(auth.Guild.Id);
            ViewData["rows"] = Notices.GetRows(auth.Guild.Id);
            ViewData["channels"] = DiscordModule.GetTextChannelsAsync(auth.Guild).GetAwaiter().GetResult();
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

            NoticesRow row = Notices.GetRow(auth.Guild.Id, channelId);

            row.Enabled = HttpContext.Request.Form["enabled"] == "on";
            row.Delay = TimeSpan.Parse(HttpContext.Request.Form["delay"]);
            row.Title = EString.FromDecoded(HttpContext.Request.Form["title"]);
            row.Footer = EString.FromDecoded(HttpContext.Request.Form["footer"]);
            row.Content = EString.FromDecoded(HttpContext.Request.Form["content"]);
            row.Text = EString.FromDecoded(HttpContext.Request.Form["text"]);
            row.Image = EString.FromDecoded(HttpContext.Request.Form["image"]);
            row.Thumbnail = EString.FromDecoded(HttpContext.Request.Form["thumbnail"]);
            row.Icon = EString.FromDecoded(HttpContext.Request.Form["icon"]);
            row.Colour = new Discord.Color(uint.Parse(HttpContext.Request.Form["colour"]));

            Notices.SaveRow(row);

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

            NoticesRow newRow = new NoticesRow(auth.Guild.Id, channel.Id);

            Notices.SaveRow(newRow);
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
            NoticesRow deleteRow = Notices.GetRow(auth.Guild.Id, channelId);
            Notices.DeleteRow(deleteRow);

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
