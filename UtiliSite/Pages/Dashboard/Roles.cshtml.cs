using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class RolesModel : PageModel
    {
        public async Task OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = Database.Premium.IsPremium(auth.Guild.Id);

            RolesRow row = Roles.GetRow(auth.Guild.Id);

            ViewData["joinRoles"] = auth.Guild.Roles.Where(x => row.JoinRoles.Contains(x.Id)).ToList();
            ViewData["nonJoinRoles"] = auth.Guild.Roles.Where(x => !row.JoinRoles.Contains(x.Id)).ToList();
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

            bool rolePersist = HttpContext.Request.Form["rolePersist"] == "on";

            RolesRow row = Roles.GetRow(auth.Guild.Id);
            bool requireDownload = !row.RolePersist && rolePersist;
            row.RolePersist = rolePersist;
            Roles.SaveRow(row);


            if (requireDownload)
            {
                MiscRow miscRow = new MiscRow(auth.Guild.Id, "RequiresUserDownload", "");
                try { Misc.SaveRow(miscRow); } catch{}
            }

            HttpContext.Response.StatusCode = 200;
        }

        public async Task OnPostAddJoinRole()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong roleId = ulong.Parse(HttpContext.Request.Form["role"]);

            RolesRow row = Roles.GetRow(auth.Guild.Id);
            if (!row.JoinRoles.Contains(roleId)) row.JoinRoles.Add(roleId);
            Roles.SaveRow(row);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public async Task OnPostRemoveJoinRole()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong roleId = ulong.Parse(HttpContext.Request.Form["role"]);

            RolesRow row = Roles.GetRow(auth.Guild.Id);
            if (row.JoinRoles.Contains(roleId)) row.JoinRoles.Remove(roleId);
            Roles.SaveRow(row);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public static string GetIsChecked(bool isChecked)
        {
            return isChecked ? "checked" : "";
        }
    }
}
