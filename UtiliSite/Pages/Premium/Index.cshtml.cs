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
        public void OnGet()
        {
            ViewData["stripepk"] = Main._config.StripePublicKey;
        }
    }
}
