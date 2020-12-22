using System;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class InactiveRoleModel : PageModel
    {
        public async Task OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = Database.Premium.IsPremium(auth.Guild.Id);

            InactiveRoleRow row = await InactiveRole.GetRowAsync(auth.Guild.Id);

            ViewData["row"] = row;
        }

        public async Task OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong inactiveRoleId = ulong.Parse(HttpContext.Request.Form["inactiveRole"]);
            ulong immuneRoleId = ulong.Parse(HttpContext.Request.Form["immuneRole"]);
            TimeSpan threshold = TimeSpan.Parse(HttpContext.Request.Form["threshold"]);
            bool inverse = bool.Parse(HttpContext.Request.Form["inverse"]);

            InactiveRoleRow row = await InactiveRole.GetRowAsync(auth.Guild.Id);

            if(!auth.Guild.Roles.Any(x => x.Id == row.RoleId))
            {
                // Inactivity data is not recorded if the role id is invalid.
                // Set the DefaultLastAction to now to assume that all users
                // were active when the data started to be recorded.
                row.DefaultLastAction = DateTime.UtcNow;
            }

            row.RoleId = inactiveRoleId;
            row.ImmuneRoleId = immuneRoleId;
            row.Threshold = threshold;
            row.Inverse = inverse;
            await InactiveRole.SaveRowAsync(row);

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
