using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Database;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc;

namespace UtiliSite.Pages.Dashboard
{
    public class JoinMessageModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if(!auth.Authenticated) return auth.Action;

            JoinMessageRow row = await JoinMessage.GetRowAsync(auth.Guild.Id);
            List<RestTextChannel> channels = await DiscordModule.GetTextChannelsAsync(auth.Guild);

            ViewData["row"] = row;
            ViewData["channels"] = channels;

            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            
            if (!auth.Authenticated) return Forbid();

            JoinMessageRow row = await JoinMessage.GetRowAsync(auth.Guild.Id);

            row.Enabled = HttpContext.Request.Form["enabled"] == "on";
            row.Direct = bool.Parse(HttpContext.Request.Form["direct"]);

            row.Title = EString.FromDecoded(HttpContext.Request.Form["title"]);
            row.Footer = EString.FromDecoded(HttpContext.Request.Form["footer"]);
            row.Content = EString.FromDecoded(HttpContext.Request.Form["content"]);
            row.Text = EString.FromDecoded(HttpContext.Request.Form["text"]);
            row.Image = EString.FromDecoded(HttpContext.Request.Form["image"]);
            row.Thumbnail = EString.FromDecoded(HttpContext.Request.Form["thumbnail"]);
            row.Icon = EString.FromDecoded(HttpContext.Request.Form["icon"]);
            row.Colour = new Discord.Color(uint.Parse(HttpContext.Request.Form["colour"].ToString().Replace("#", ""), System.Globalization.NumberStyles.HexNumber));

            await JoinMessage.SaveRowAsync(row);

            return new OkResult();
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
