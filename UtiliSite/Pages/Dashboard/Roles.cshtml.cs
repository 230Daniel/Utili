using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class RolesModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if(!auth.Authenticated) return auth.Action;

            RolesRow row = await Roles.GetRowAsync(auth.Guild.Id);
            List<RestRole> joinRoles = auth.Guild.Roles.Where(x => row.JoinRoles.Contains(x.Id)).ToList();
            List<RestRole> nonJoinRoles = auth.Guild.Roles.Where(x => !row.JoinRoles.Contains(x.Id)).ToList();

            ViewData["joinRoles"] = joinRoles;
            ViewData["nonJoinRoles"] = nonJoinRoles;
            ViewData["row"] = row;
            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

                bool rolePersist = HttpContext.Request.Form["rolePersist"] == "on";

            RolesRow row = await Roles.GetRowAsync(auth.Guild.Id);
            bool requireDownload = !row.RolePersist && rolePersist;
            row.RolePersist = rolePersist;
            await Roles.SaveRowAsync(row);


            if (requireDownload)
            {
                MiscRow miscRow = new MiscRow(auth.Guild.Id, "RequiresUserDownload", "");
                try { await Misc.SaveRowAsync(miscRow); } catch{}
            }

            return new OkResult();
        }

        public async Task<ActionResult> OnPostAddJoinRole()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

            ulong roleId = ulong.Parse(HttpContext.Request.Form["role"]);

            RolesRow row = await Roles.GetRowAsync(auth.Guild.Id);
            if (!row.JoinRoles.Contains(roleId)) row.JoinRoles.Add(roleId);
            
            await Roles.SaveRowAsync(row);

            return new RedirectResult(Request.Path);
        }

        public async Task<ActionResult> OnPostRemoveJoinRole()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);

            if (!auth.Authenticated) return Forbid();

            ulong roleId = ulong.Parse(HttpContext.Request.Form["role"]);

            RolesRow row = await Roles.GetRowAsync(auth.Guild.Id);
            if (row.JoinRoles.Contains(roleId)) row.JoinRoles.Remove(roleId);
            await Roles.SaveRowAsync(row);

            return new RedirectResult(Request.Path);
        }

        public static string GetIsChecked(bool isChecked)
        {
            return isChecked ? "checked" : "";
        }
    }
}
