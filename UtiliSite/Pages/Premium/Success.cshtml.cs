using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Premium
{
    public class SuccessModel : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);
            ViewData["auth"] = auth;
            if(!auth.Authenticated) return Forbid();

            await Task.Delay(2000);

            List<SubscriptionsRow> subscriptions = await Subscriptions.GetRowsAsync(userId: auth.User.Id);
            ViewData["subscriptions"] = subscriptions.Count;
            ViewData["slots"] = subscriptions.Sum(x => x.Slots);

            if (subscriptions.Count == 0)
            {
                return Redirect(HttpContext.Request.Host.ToString());
            }

            return new PageResult();
        }
    }
}
