using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class MessagePinningModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if (!auth.Authenticated) return auth.Action;

            MessagePinningRow row = await MessagePinning.GetRowAsync(auth.Guild.Id);
            List<RestTextChannel> channels = await DiscordModule.GetTextChannelsAsync(auth.Guild);

            ViewData["row"] = row;
            ViewData["channels"] = channels;

            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return auth.Action;

            ulong pinChannelId = ulong.Parse(HttpContext.Request.Form["pinChannel"]);
            bool pin = HttpContext.Request.Form["pin"] == "on";

            MessagePinningRow row = await MessagePinning.GetRowAsync(auth.Guild.Id);
            row.Pin = pin;
            row.PinChannelId = pinChannelId;
            await MessagePinning.SaveRowAsync(row);

            return new OkResult();
        }
    }
}
