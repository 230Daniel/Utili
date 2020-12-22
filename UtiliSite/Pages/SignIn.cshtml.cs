using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
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
