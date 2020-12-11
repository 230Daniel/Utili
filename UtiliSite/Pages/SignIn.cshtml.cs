using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Rest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages
{
    public class SignInModel : PageModel
    {
        public async Task OnGet()
        {
            AuthenticationProperties authProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Content("~/"),
                AllowRefresh = true,
                IsPersistent = true
            };

            await HttpContext.ChallengeAsync("Discord", authProperties);
        }
    }
}
