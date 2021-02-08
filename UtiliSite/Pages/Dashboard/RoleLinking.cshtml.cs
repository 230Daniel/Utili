using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class RoleLinkingModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if(!auth.Authenticated) return auth.Action;

            List<RoleLinkingRow> rows = await RoleLinking.GetRowsAsync(auth.Guild.Id);
            foreach(RoleLinkingRow row in rows.Where(x => auth.Guild.Roles.All(y => y.Id != x.RoleId))) await row.DeleteAsync();
            if (!(bool) ViewData["premium"] && rows.Count > 2)
            {
                foreach(RoleLinkingRow row in rows.TakeLast(rows.Count - 2)) await row.DeleteAsync();
                rows = rows.Take(2).ToList();
            }
            ViewData["rows"] = rows;

            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if (!auth.Authenticated) return Forbid();

            ulong linkId = ulong.Parse(HttpContext.Request.Form["link"]);
            ulong linkedRoleId = ulong.Parse(HttpContext.Request.Form["linkedRole"]);
            int mode = int.Parse(HttpContext.Request.Form["mode"]);

            RoleLinkingRow row = await RoleLinking.GetRowAsync(auth.Guild.Id, linkId);
            row.LinkedRoleId = linkedRoleId;
            row.Mode = mode;
            await RoleLinking.SaveRowAsync(row);

            return new OkResult();
        }

        public async Task<ActionResult> OnPostAddRoleGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if (!auth.Authenticated) return Forbid();

            ulong roleId = ulong.Parse(HttpContext.Request.Form["role"]);
            RoleLinkingRow row = new RoleLinkingRow(auth.Guild.Id, roleId, 0) {Mode = 0};
            await RoleLinking.SaveRowAsync(row);

            MiscRow miscRow = new MiscRow(auth.Guild.Id, "RequiresUserDownload", "");
            try { await Misc.SaveRowAsync(miscRow); } catch { }

            return new RedirectResult(Request.Path);
        }

        public async Task<ActionResult> OnPostAddRoleRemove()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if (!auth.Authenticated) return Forbid();

            ulong roleId = ulong.Parse(HttpContext.Request.Form["role"]);
            RoleLinkingRow row = new RoleLinkingRow(auth.Guild.Id, roleId, 0) {Mode = 2};
            await RoleLinking.SaveRowAsync(row);

            MiscRow miscRow = new MiscRow(auth.Guild.Id, "RequiresUserDownload", "");
            try { await Misc.SaveRowAsync(miscRow); } catch { }

            return new RedirectResult(Request.Path);
        }

        public async Task<ActionResult> OnPostRemove()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if (!auth.Authenticated) return Forbid();

            ulong linkId = ulong.Parse(HttpContext.Request.Form["link"]);
            RoleLinkingRow row = await RoleLinking.GetRowAsync(auth.Guild.Id, linkId);
            await RoleLinking.DeleteRowAsync(row);

            return new RedirectResult(Request.Path);
        }
    }
}
