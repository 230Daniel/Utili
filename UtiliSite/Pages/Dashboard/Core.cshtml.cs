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
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if(!auth.Authenticated) return;
            
            CoreRow row = await Core.GetRowAsync(auth.Guild.Id);
            List<RestTextChannel> channels = await DiscordModule.GetTextChannelsAsync(auth.Guild);
            List<RestTextChannel> excludedChannels = channels.Where(x => row.ExcludedChannels.Contains(x.Id)).OrderBy(x => x.Position).ToList();
            List<RestTextChannel> nonExcludedChannels = channels.Where(x => !row.ExcludedChannels.Contains(x.Id)).OrderBy(x => x.Position).ToList();
            string nickname = await DiscordModule.GetBotNicknameAsync(auth.Guild.Id);

            ViewData["row"] = row;
            ViewData["excludedChannels"] = excludedChannels;
            ViewData["nonExcludedChannels"] = nonExcludedChannels;
            ViewData["nickname"] = nickname;
        }

        public async Task OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            CoreRow row = await Core.GetRowAsync(auth.Guild.Id);
            row.Prefix = EString.FromDecoded(HttpContext.Request.Form["prefix"]);
            row.EnableCommands = HttpContext.Request.Form["enableCommands"] == "on";
            await Core.SaveRowAsync(row);

            string nickname = HttpContext.Request.Form["nickname"];
            if (nickname != (string) ViewData["nickname"])
            {
                await DiscordModule.SetNicknameAsync(auth.Guild.Id, nickname);
            }

            HttpContext.Response.StatusCode = 200;
        }

        public async Task OnPostExclude()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            CoreRow row = await Core.GetRowAsync(auth.Guild.Id);
            if (!row.ExcludedChannels.Contains(channelId))
            {
                row.ExcludedChannels.Add(channelId);
            }

            await Core.SaveRowAsync(row);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public async Task OnPostRemove()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            CoreRow row = await Core.GetRowAsync(auth.Guild.Id);
            row.ExcludedChannels.RemoveAll(x => x == channelId);
            await Core.SaveRowAsync(row);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }
    }
}
