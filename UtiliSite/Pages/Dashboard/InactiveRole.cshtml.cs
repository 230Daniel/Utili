using System;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class InactiveRoleModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if(!auth.Authenticated) return auth.Action;

            InactiveRoleRow row = await InactiveRole.GetRowAsync(auth.Guild.Id);
            ViewData["row"] = row;

            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if (!auth.Authenticated) return Forbid();

            ulong inactiveRoleId = ulong.Parse(HttpContext.Request.Form["inactiveRole"]);
            ulong immuneRoleId = ulong.Parse(HttpContext.Request.Form["immuneRole"]);
            TimeSpan threshold = TimeSpan.Parse(HttpContext.Request.Form["threshold"]);
            bool inverse = bool.Parse(HttpContext.Request.Form["inverse"]);

            InactiveRoleRow row = await InactiveRole.GetRowAsync(auth.Guild.Id);

            if(auth.Guild.Roles.All(x => x.Id != row.RoleId))
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

            return new OkResult();
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
