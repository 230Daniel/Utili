using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class CoreModel : PageModel
    {
        public async Task OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = Database.Premium.IsPremium(auth.Guild.Id);

            CoreRow row = Core.GetRow(auth.Guild.Id);
            ViewData["row"] = row;
            ViewData["nickname"] = await DiscordModule.GetBotNicknameAsync(auth.Guild.Id);

            List<RestTextChannel> textChannels = await DiscordModule.GetTextChannelsAsync(auth.Guild);
            ViewData["excludedChannels"] =  textChannels.Where(x => row.ExcludedChannels.Contains(x.Id)).OrderBy(x => x.Position).ToList();
            ViewData["nonExcludedChannels"] =  textChannels.Where(x => !row.ExcludedChannels.Contains(x.Id)).OrderBy(x => x.Position).ToList();
        }

        public async Task OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            CoreRow row = Core.GetRow(auth.Guild.Id);
            row.Prefix = EString.FromDecoded(HttpContext.Request.Form["prefix"]);
            row.EnableCommands = HttpContext.Request.Form["enableCommands"] == "on";
            Core.SaveRow(row);

            string nickname = HttpContext.Request.Form["nickname"];
            if (nickname != (string) ViewData["nickname"])
            {
                await DiscordModule.SetNicknameAsync(auth.Guild.Id, nickname);
            }

            HttpContext.Response.StatusCode = 200;
        }

        public async Task OnPostExclude()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            CoreRow row = Core.GetRow(auth.Guild.Id);
            if (!row.ExcludedChannels.Contains(channelId))
            {
                row.ExcludedChannels.Add(channelId);
            }
            Core.SaveRow(row);

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

            CoreRow row = Core.GetRow(auth.Guild.Id);
            row.ExcludedChannels.RemoveAll(x => x == channelId);
            Core.SaveRow(row);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }
    }
}
