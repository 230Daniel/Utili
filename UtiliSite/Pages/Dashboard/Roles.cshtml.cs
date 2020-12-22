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
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = await Database.Data.Premium.IsGuildPremiumAsync(auth.Guild.Id);

            RolesRow row = await Roles.GetRowAsync(auth.Guild.Id);

            ViewData["joinRoles"] = auth.Guild.Roles.Where(x => row.JoinRoles.Contains(x.Id)).ToList();
            ViewData["nonJoinRoles"] = auth.Guild.Roles.Where(x => !row.JoinRoles.Contains(x.Id)).ToList();
            ViewData["row"] = row;
        }

        public async Task OnPost()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

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

            HttpContext.Response.StatusCode = 200;
        }

        public async Task OnPostAddJoinRole()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong roleId = ulong.Parse(HttpContext.Request.Form["role"]);

            RolesRow row = await Roles.GetRowAsync(auth.Guild.Id);
            if (!row.JoinRoles.Contains(roleId)) row.JoinRoles.Add(roleId);
            await Roles.SaveRowAsync(row);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public async Task OnPostRemoveJoinRole()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong roleId = ulong.Parse(HttpContext.Request.Form["role"]);

            RolesRow row = await Roles.GetRowAsync(auth.Guild.Id);
            if (row.JoinRoles.Contains(roleId)) row.JoinRoles.Remove(roleId);
            await Roles.SaveRowAsync(row);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public static string GetIsChecked(bool isChecked)
        {
            return isChecked ? "checked" : "";
        }
    }
}
