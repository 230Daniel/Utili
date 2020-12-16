using System.Collections.Generic;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            if (HttpContext.Request.RouteValues.TryGetValue("guild", out _))
            {
                Response.Redirect(RedirectHelper.AddToUrl(HttpContext.Request.Path, "core"));
                return;
            }

            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;

            ViewData["avatarUrl"] = auth.Client.CurrentUser.GetAvatarUrl();
            ViewData["guilds"] = DiscordModule.GetManageableGuilds(auth.Client);
        }
    }
}
