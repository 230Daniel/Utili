using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Org.BouncyCastle.Asn1.Cmp;
using System.Text.Json;

namespace UtiliSite.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext);
            if(!auth.Authenticated) return;

            if (auth.Guild != null)
            {
                ViewData["guild"] = auth.Guild.Name;
            }
        }
    }
}
