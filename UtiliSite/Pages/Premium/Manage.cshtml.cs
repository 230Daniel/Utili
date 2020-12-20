using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Database.Data;

namespace UtiliSite.Pages.Premium
{
    public class ManageModel : PageModel
    {
        public async Task OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);
            ViewData["auth"] = auth;
            if (!auth.Authenticated) return;

            ViewData["rows"] = Database.Data.Premium.GetUserRows(auth.User.Id);
        }
    }
}
