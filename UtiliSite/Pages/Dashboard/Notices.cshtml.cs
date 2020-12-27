using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Database;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc;

namespace UtiliSite.Pages.Dashboard
{
    public class NoticesModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if (!auth.Authenticated) return RedirectToPage("Index");

            List<NoticesRow> rows = await Notices.GetRowsAsync(auth.Guild.Id);
            List<RestTextChannel> channels = await DiscordModule.GetTextChannelsAsync(auth.Guild);

            ViewData["rows"] = rows;
            ViewData["channels"] = channels;

            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

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

            return new OkResult();
        }

        public async Task<ActionResult> OnPostAdd()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);
            RestTextChannel channel = auth.Guild.GetTextChannelAsync(channelId).GetAwaiter().GetResult();

            NoticesRow row = new NoticesRow(auth.Guild.Id, channel.Id);

            try { await Notices.SaveRowAsync(row); }
            catch { }

            return new RedirectResult(Request.Path);
        }

        public async Task<ActionResult> OnPostRemove()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);
            NoticesRow row = await Notices.GetRowAsync(auth.Guild.Id, channelId);
            await Notices.DeleteRowAsync(row);

            return new RedirectResult(Request.Path);
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
