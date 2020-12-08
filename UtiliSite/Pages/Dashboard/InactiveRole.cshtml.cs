using System;
using System.Linq;
using Database.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class InactiveRoleModel : PageModel
    {
        public void OnGet()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = Database.Premium.IsPremium(auth.Guild.Id);

            InactiveRoleRow row = InactiveRole.GetRow(auth.Guild.Id);

            ViewData["row"] = row;
        }

        public void OnPost()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong inactiveRoleId = ulong.Parse(HttpContext.Request.Form["inactiveRole"]);
            ulong immuneRoleId = ulong.Parse(HttpContext.Request.Form["immuneRole"]);
            TimeSpan threshold = TimeSpan.Parse(HttpContext.Request.Form["threshold"]);
            bool inverse = bool.Parse(HttpContext.Request.Form["inverse"]);

            InactiveRoleRow row = InactiveRole.GetRow(auth.Guild.Id);
            row.RoleId = inactiveRoleId;
            row.ImmuneRoleId = immuneRoleId;
            row.Threshold = threshold;
            row.Inverse = inverse;
            InactiveRole.SaveRow(row);

            HttpContext.Response.StatusCode = 200;
        }

        public static string GetIsRoleSelected(ulong roleOne, ulong roleTwo)
        {
            return roleTwo == roleOne ? "selected" : "";
        }

        public static string GetIsBoolSelected(bool boolOne, bool boolTwo)
        {
            return boolTwo == boolOne ? "selected" : "";
        }
    }
}
