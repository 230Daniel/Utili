using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Database.Data;
using Microsoft.AspNetCore.Mvc;

namespace UtiliSite.Pages.Premium
{
    public class IndexModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            ViewData["auth"] = auth;
            if(!auth.Authenticated) return auth.Action;

            (string, bool) currency = await PaymentsController.GetCustomerCurrencyAsync(auth.UserRow.CustomerId, Request);
            ViewData["currency"] = currency.Item1;
            ViewData["forceCurrency"] = currency.Item2;
            ViewData["showCurrency"] = true;

            ViewData["subscriptions"] = await Subscriptions.GetRowsAsync(userId: auth.User.Id);

            return Page();
        }
    }
}
