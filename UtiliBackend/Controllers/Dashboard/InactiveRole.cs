using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Database;
using Database.Data;

namespace UtiliBackend.Controllers.Dashboard
{
    public class InactiveRole : Controller
    {
        [HttpGet("dashboard/{guildId}/inactiverole")]
        public async Task<ActionResult> Get([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            InactiveRoleRow row = await Database.Data.InactiveRole.GetRowAsync(auth.Guild.Id);
            return new JsonResult(new InactiveRoleBody(row));
        }

        [HttpPost("dashboard/{guildId}/inactiverole")]
        public async Task<ActionResult> Post([Required] ulong guildId, [Required] [FromBody] InactiveRoleBody body)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            InactiveRoleRow row = await Database.Data.InactiveRole.GetRowAsync(auth.Guild.Id);
            row.RoleId = ulong.Parse(body.RoleId);
            row.ImmuneRoleId = ulong.Parse(body.ImmuneRoleId);
            row.Threshold = XmlConvert.ToTimeSpan(body.Threshold);
            row.Inverse = body.Inverse;
            row.AutoKick = body.AutoKick;
            row.AutoKickThreshold = XmlConvert.ToTimeSpan(body.AutoKickThreshold);
            await row.SaveAsync();

            return new OkResult();
        }
    }

    public class InactiveRoleBody
    {
        public string RoleId { get; set; }
        public string ImmuneRoleId { get; set; }
        public string Threshold { get; set; }
        public bool Inverse { get; set; }

        public bool AutoKick { get; set; }
        public string AutoKickThreshold { get; set; }

        public InactiveRoleBody(InactiveRoleRow row)
        {
            RoleId = row.RoleId.ToString();
            ImmuneRoleId = row.ImmuneRoleId.ToString();
            Threshold = XmlConvert.ToString(row.Threshold);
            Inverse = row.Inverse;

            AutoKick = row.AutoKick;
            AutoKickThreshold = XmlConvert.ToString(row.AutoKickThreshold);
        }

        public InactiveRoleBody() { }
    }
}