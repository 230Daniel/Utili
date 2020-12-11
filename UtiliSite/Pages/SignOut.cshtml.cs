using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Rest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages
{
    public class SignOutModel : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            await HttpContext.SignOutAsync();
            return Redirect(Url.Content("~/"));
        }
    }
}
