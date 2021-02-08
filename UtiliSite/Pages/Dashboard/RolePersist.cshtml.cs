using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class RolePersistModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if (!auth.Authenticated) return auth.Action;

            RolePersistRow row = await RolePersist.GetRowAsync(auth.Guild.Id);
            List<RestRole> excludedRoles = auth.Guild.Roles.Where(x => row.ExcludedRoles.Contains(x.Id)).OrderBy(x => -x.Position).ToList();
            List<RestRole> nonExcludedRoles = auth.Guild.Roles.Where(x => !row.ExcludedRoles.Contains(x.Id)).OrderBy(x => -x.Position).ToList();

            ViewData["row"] = row;
            ViewData["excludedRoles"] = excludedRoles;
            ViewData["nonExcludedRoles"] = nonExcludedRoles;

            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if (!auth.Authenticated) return Forbid();

            RolePersistRow row = await RolePersist.GetRowAsync(auth.Guild.Id);
            bool previous = row.Enabled;
            row.Enabled = HttpContext.Request.Form["enable"] == "on";
            await RolePersist.SaveRowAsync(row);

            if (!previous && row.Enabled)
            {
                MiscRow miscRow = new MiscRow(auth.Guild.Id, "RequiresUserDownload", "");
                try { await Misc.SaveRowAsync(miscRow); } catch { }
            }

            return new OkResult();
        }

        public async Task<ActionResult> OnPostExclude()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if (!auth.Authenticated) return Forbid();

            ulong roleId = ulong.Parse(HttpContext.Request.Form["role"]);
            RolePersistRow row = await RolePersist.GetRowAsync(auth.Guild.Id);
            if (!row.ExcludedRoles.Contains(roleId))
            {
                row.ExcludedRoles.Add(roleId);
            }
            await RolePersist.SaveRowAsync(row);

            return new RedirectResult(Request.Path);
        }

        public async Task<ActionResult> OnPostRemove()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if (!auth.Authenticated) return Forbid();

            ulong roleId = ulong.Parse(HttpContext.Request.Form["role"]);
            RolePersistRow row = await RolePersist.GetRowAsync(auth.Guild.Id);
            if (row.ExcludedRoles.Contains(roleId))
            {
                row.ExcludedRoles.Remove(roleId);
            }
            await RolePersist.SaveRowAsync(row);

            return new RedirectResult(Request.Path);
        }
    }
}
