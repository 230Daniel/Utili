using System.Collections.Generic;
using System.Linq;
using Database;
using Database.Data;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class RolesModel : PageModel
    {
        public void OnGet()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;

            ViewData["guild"] = auth.Guild;
            ViewData["Title"] = $"{auth.Guild.Name} - ";

            List<RolesRow> rows = Roles.GetRows(auth.Guild.Id);
            RolesRow row = rows.Count == 0 ? new RolesRow() : rows.First();

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

            List<RolesRow> rows = Roles.GetRows(auth.Guild.Id);
            RolesRow row = rows.Count == 0 ? new RolesRow() : rows.First();
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

            List<RolesRow> rows = Roles.GetRows(auth.Guild.Id);
            RolesRow row = rows.Count == 0 ? new RolesRow() : rows.First();
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

            List<RolesRow> rows = Roles.GetRows(auth.Guild.Id);
            RolesRow row = rows.Count == 0 ? new RolesRow() : rows.First();
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
