using System.Collections.Generic;
using System.Linq;
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

            List<SubscriptionsRow> subscriptions = await Subscriptions.GetRowsAsync(userId: auth.User.Id, onlyValid: true);
            ViewData["subscriptions"] = subscriptions.Count;
            ViewData["slots"] = subscriptions.Sum(x => x.Slots);

            return new PageResult();
        }
    }
}
