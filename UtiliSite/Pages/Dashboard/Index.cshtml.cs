using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            //ViewData["guilds"] = new List<RestGuild>(); // avoid null ref exception on page

            if (HttpContext.Request.RouteValues.TryGetValue("guild", out _))
            {
                return new RedirectResult(RedirectHelper.AddToUrl(HttpContext.Request.Path, "core"));
            }

            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if(!auth.Authenticated) return auth.Action;

            ViewData["auth"] = auth;
            ViewData["guilds"] = await DiscordModule.GetManageableGuildsAsync(auth.Client);

            return Page();
        }
    }
}
