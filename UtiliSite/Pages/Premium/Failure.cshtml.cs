using System.Threading.Tasks;
using Database.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Premium
{
    public class FailureModel : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            ViewData["auth"] = auth;
            if(!auth.Authenticated) return auth.Action;

            ViewData["subscriptions"] = await Subscriptions.GetRowsAsync(userId: auth.User.Id);
            return new PageResult();
        }
    }
}
