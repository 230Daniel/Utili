using System;
using System.Linq;
using Database.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Database;

namespace UtiliSite.Pages.Dashboard
{
    public class JoinMessageModel : PageModel
    {
        public void OnGet()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;

            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = Database.Premium.IsPremium(auth.Guild.Id);
            ViewData["row"] = JoinMessage.GetRow(auth.Guild.Id);
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

            JoinMessageRow row = JoinMessage.GetRow(auth.Guild.Id);

            row.Enabled = HttpContext.Request.Form["enabled"] == "on";
            row.Direct = bool.Parse(HttpContext.Request.Form["direct"]);

            row.Title = EString.FromDecoded(HttpContext.Request.Form["title"]);
            row.Footer = EString.FromDecoded(HttpContext.Request.Form["footer"]);
            row.Content = EString.FromDecoded(HttpContext.Request.Form["content"]);
            row.Text = EString.FromDecoded(HttpContext.Request.Form["text"]);
            row.Image = EString.FromDecoded(HttpContext.Request.Form["image"]);
            row.Thumbnail = EString.FromDecoded(HttpContext.Request.Form["thumbnail"]);
            row.Icon = EString.FromDecoded(HttpContext.Request.Form["icon"]);
            row.Colour = new Discord.Color(uint.Parse(HttpContext.Request.Form["colour"]));

            JoinMessage.SaveRow(row);

            HttpContext.Response.StatusCode = 200;
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