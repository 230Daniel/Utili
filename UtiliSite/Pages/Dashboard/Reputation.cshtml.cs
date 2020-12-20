using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class ReputationModel : PageModel
    {
        public async Task OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = Database.Premium.IsPremium(auth.Guild.Id);

            ViewData["row"] = Reputation.GetRow(auth.Guild.Id);
        }

        public async Task OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ReputationRow row = Reputation.GetRow(auth.Guild.Id);
            (IEmote, int) emote = row.Emotes.First(x => x.Item1.ToString() == HttpContext.Request.Form["emote"]);
            int value = int.Parse(HttpContext.Request.Form["value"]);

            row.Emotes.Remove(emote);
            row.Emotes.Add((emote.Item1, value));
            Reputation.SaveRow(row);

            HttpContext.Response.StatusCode = 200;
        }

        public async Task OnPostRemove()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ReputationRow row = Reputation.GetRow(auth.Guild.Id);
            (IEmote, int) emote = row.Emotes.First(x => x.Item1.ToString() == HttpContext.Request.Form["emote"]);
            row.Emotes.Remove(emote);

            Reputation.SaveRow(row);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }
    }
}
