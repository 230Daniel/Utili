using System.Linq;
using Database.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class RolesModel : PageModel
    {
        public void OnGet()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;

            RolesRow row = Roles.GetRow(auth.Guild.Id);

            ViewData["joinRoles"] = auth.Guild.Roles.Where(x => row.JoinRoles.Contains(x.Id)).ToList();
            ViewData["nonJoinRoles"] = auth.Guild.Roles.Where(x => !row.JoinRoles.Contains(x.Id)).ToList();
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

            bool rolePersist = HttpContext.Request.Form["rolePersist"] == "on";

            RolesRow row = Roles.GetRow(auth.Guild.Id);
            row.RolePersist = rolePersist;
            Roles.SaveRow(row);

            HttpContext.Response.StatusCode = 200;
        }

        public void OnPostAddJoinRole()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong roleId = ulong.Parse(HttpContext.Request.Form["roleId"]);

            RolesRow row = Roles.GetRow(auth.Guild.Id);
            if (!row.JoinRoles.Contains(roleId)) row.JoinRoles.Add(roleId);
            Roles.SaveRow(row);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public void OnPostRemoveJoinRole()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong roleId = ulong.Parse(HttpContext.Request.Form["roleId"]);

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
