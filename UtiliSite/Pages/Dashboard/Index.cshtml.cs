using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        public async Task OnGet()
        {
            ViewData["guilds"] = new List<RestGuild>(); // avoid null ref exception on page

            if (HttpContext.Request.RouteValues.TryGetValue("guild", out _))
            {
                Response.Redirect(RedirectHelper.AddToUrl(HttpContext.Request.Path, "core"));
                return;
            }

            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;
            ViewData["auth"] = auth;

            ViewData["guilds"] = await DiscordModule.GetManageableGuildsAsync(auth.Client);
        }
    }
}
