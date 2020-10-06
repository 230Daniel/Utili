using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        public string Code { get; set; }

        public void OnGet()
        {
            if (true)
            {
                Code = Request.Query["code"].ToString();
            }
            
            if (string.IsNullOrEmpty(Code))
            {
                Response.Redirect("https://discord.com/api/oauth2/authorize?client_id=655155797260501039&redirect_uri=https%3A%2F%2Flocalhost%3A44347%2FDashboard&response_type=code&scope=identify%20email%20guilds");
            }

            ViewData["code"] = Code;
        }
    }
}
