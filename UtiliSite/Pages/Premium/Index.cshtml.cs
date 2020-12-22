using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Database.Data;

namespace UtiliSite.Pages.Premium
{
    public class IndexModel : PageModel
    {
        public async Task OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);
            ViewData["auth"] = auth;
            if(!auth.Authenticated) return;

            (string, bool) currency = await PaymentsController.GetCustomerCurrencyAsync(auth.UserRow.CustomerId, Request);
            ViewData["currency"] = currency.Item1;
            ViewData["forceCurrency"] = currency.Item2;
            ViewData["showCurrency"] = true;

            List<SubscriptionsRow> subscriptions = await Subscriptions.GetRowsAsync(userId: auth.User.Id);
            ViewData["subscriptions"] = subscriptions.Count;
            ViewData["slots"] = subscriptions.Sum(x => x.Slots);
        }
    }
}
