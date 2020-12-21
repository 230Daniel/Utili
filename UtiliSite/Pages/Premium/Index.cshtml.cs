using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Premium
{
    public class IndexModel : PageModel
    {
        public async Task OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);
            ViewData["auth"] = auth;
            if(!auth.Authenticated) return;

            (string, bool) currency = await PaymentsController.GetCustomerCurrencyAsync(auth.UserRow.CustomerId, HttpContext);
            ViewData["currency"] = currency.Item1;
            ViewData["forceCurrency"] = currency.Item2;
        }
    }
}
