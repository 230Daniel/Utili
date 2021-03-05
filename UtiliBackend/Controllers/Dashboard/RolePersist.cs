using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;

namespace UtiliBackend.Controllers.Dashboard
{
    public class RolePersist : Controller
    {
        [HttpGet("dashboard/{guildId}/rolepersist")]
        public async Task<ActionResult> Get([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            RolePersistRow row = await Database.Data.RolePersist.GetRowAsync(auth.Guild.Id);
            return new JsonResult(new RolePersistBody(row));
        }

        [HttpPost("dashboard/{guildId}/rolepersist")]
        public async Task<ActionResult> Post([Required] ulong guildId, [Required] [FromBody] RolePersistBody body)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            RolePersistRow row = await Database.Data.RolePersist.GetRowAsync(auth.Guild.Id);
            row.Enabled = body.Enabled;
            row.ExcludedRoles = body.ExcludedRoles.Select(ulong.Parse).ToList();
            await row.SaveAsync();

            return new OkResult();
        }
    }

    public class RolePersistBody
    {
        public bool Enabled { get; set; }
        public List<string> ExcludedRoles { get; set; }

        public RolePersistBody(RolePersistRow row)
        {
            Enabled = row.Enabled;
            ExcludedRoles = row.ExcludedRoles.Select(x => x.ToString()).ToList();
        }

        public RolePersistBody() { }
    }
}
