using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;

namespace UtiliBackend.Controllers.Dashboard
{
    public class RoleLinking : Controller
    {
        [HttpGet("dashboard/{guildId}/rolelinking")]
        public async Task<ActionResult> Get([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            List<RoleLinkingRow> rows = await Database.Data.RoleLinking.GetRowsAsync(auth.Guild.Id);
            return new JsonResult(new RoleLinkingBody(rows));
        }

        [HttpPost("dashboard/{guildId}/rolelinking")]
        public async Task<ActionResult> Post([Required] ulong guildId, [Required] [FromBody] RoleLinkingBody body)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            List<RoleLinkingRow> rows = await Database.Data.RoleLinking.GetRowsAsync(auth.Guild.Id);
            foreach (RoleLinkingRowBody bodyRow in body.Rows)
            {
                RoleLinkingRow row;
                if (rows.Any(x => x.LinkId == ulong.Parse(bodyRow.LinkId)))
                    row = rows.First(x => x.LinkId == ulong.Parse(bodyRow.LinkId));
                else
                    row = await Database.Data.RoleLinking.GetRowAsync(auth.Guild.Id, ulong.Parse(bodyRow.LinkId));

                row.RoleId = ulong.Parse(bodyRow.RoleId);
                row.LinkedRoleId = ulong.Parse(bodyRow.RoleId);
                row.Mode = bodyRow.Mode;
                await row.SaveAsync();
            }

            foreach (RoleLinkingRow row in rows.Where(x => body.Rows.All(y => ulong.Parse(y.LinkId) != x.LinkId)))
                await row.DeleteAsync();

            return new OkResult();
        }
    }

    public class RoleLinkingBody
    {
        public List<RoleLinkingRowBody> Rows { get; set; }

        public RoleLinkingBody(List<RoleLinkingRow> rows)
        {
            Rows = rows.Select(x => new RoleLinkingRowBody(x)).ToList();
        }

        public RoleLinkingBody() { }
    }

    public class RoleLinkingRowBody
    {
        public string LinkId { get; set; }
        public string RoleId { get; set; }
        public string LinkedRoleId { get; set; }
        public int Mode { get; set; }

        public RoleLinkingRowBody(RoleLinkingRow row)
        {
            LinkId = row.LinkId.ToString();
            RoleId = row.RoleId.ToString();
            LinkedRoleId = row.LinkedRoleId.ToString();
            Mode = row.Mode;
        }

        public RoleLinkingRowBody() { }
    }
}
