using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;

namespace UtiliBackend.Controllers.Dashboard
{
    public class JoinRoles : Controller
    {
        [HttpGet("dashboard/{guildId}/joinroles")]
        public async Task<ActionResult> Get([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            JoinRolesRow row = await Database.Data.JoinRoles.GetRowAsync(auth.Guild.Id);
            return new JsonResult(new JoinRolesBody(row));
        }

        [HttpPost("dashboard/{guildId}/joinroles")]
        public async Task<ActionResult> Post([Required] ulong guildId, [Required] [FromBody] JoinRolesBody body)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            JoinRolesRow row = await Database.Data.JoinRoles.GetRowAsync(auth.Guild.Id);
            row.WaitForVerification = body.WaitForVerification;
            row.JoinRoles = body.JoinRoles.Select(ulong.Parse).ToList();
            await row.SaveAsync();

            return new OkResult();
        }
    }

    public class JoinRolesBody
    {
        public bool WaitForVerification { get; set; }
        public List<string> JoinRoles { get; set; }

        public JoinRolesBody(JoinRolesRow row)
        {
            WaitForVerification = row.WaitForVerification;
            JoinRoles = row.JoinRoles.Select(x => x.ToString()).ToList();
        }

        public JoinRolesBody() { }
    }
}
