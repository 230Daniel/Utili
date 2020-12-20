using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class MessagePinningModel : PageModel
    {
        public async Task OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = Database.Premium.IsPremium(auth.Guild.Id);

            MessagePinningRow row = MessagePinning.GetRow(auth.Guild.Id);

            ViewData["channels"] = await DiscordModule.GetTextChannelsAsync(auth.Guild);
            ViewData["row"] = row;
        }

        public async Task OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong pinChannelId = ulong.Parse(HttpContext.Request.Form["pinChannel"]);
            bool pin = HttpContext.Request.Form["pin"] == "on";

            MessagePinningRow row = MessagePinning.GetRow(auth.Guild.Id);
            row.Pin = pin;
            row.PinChannelId = pinChannelId;
            MessagePinning.SaveRow(row);

            HttpContext.Response.StatusCode = 200;
        }
    }
}
