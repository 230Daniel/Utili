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
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            ViewData["auth"] = auth;
            if(!auth.Authenticated) return auth.Action;

            await Task.Delay(2000);

            List<SubscriptionsRow> subscriptions = await Subscriptions.GetRowsAsync(userId: auth.User.Id);
            ViewData["subscriptions"] = subscriptions;

            if (subscriptions.Count(x => x.IsValid) == 0)
                return Redirect("failure");
            return new PageResult();
        }
    }
}
