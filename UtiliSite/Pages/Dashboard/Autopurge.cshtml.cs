using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Database.Data;
using Discord.Rest;

namespace UtiliSite.Pages.Dashboard
{
    public class AutopurgeModel : PageModel
    {
        public void OnGet()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);

            if(!auth.Authenticated) return;

            ViewData["mainDashboardUrl"] = RedirectHelper.AddToUrl(HttpContext.Request.Host.ToString(), "dashboard");
            ViewData["guildName"] = auth.Guild.Name;
            ViewData["guild"] = auth.Guild;
            ViewData["Title"] = $"{auth.Guild.Name} - ";

            List<AutopurgeRow> autopurgeRows = Autopurge.GetRows(auth.Guild.Id);
            ViewData["autopurgeRows"] = autopurgeRows;
        }

        public void OnPost()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            string timespanString = HttpContext.Request.Form["timespan"];

            HttpContext.Response.StatusCode = 200;
        }

        public static string GetIsSelected(int mode, AutopurgeRow row)
        {
            if (row.Mode == mode) return "selected";
            return "";
        }
    }
}
