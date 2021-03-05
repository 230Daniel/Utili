using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Database;
using Database.Data;
using Newtonsoft.Json;

namespace UtiliBackend.Controllers.Dashboard
{
    public class VoiceRoles : Controller
    {
        [HttpGet("dashboard/{guildId}/voiceroles")]
        public async Task<ActionResult> Get([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            List<VoiceRolesRow> rows = await Database.Data.VoiceRoles.GetRowsAsync(auth.Guild.Id);
            return new JsonResult(new VoiceRolesBody(rows));
        }

        [HttpPost("dashboard/{guildId}/voiceroles")]
        public async Task<ActionResult> Post([Required] ulong guildId, [Required] [FromBody] VoiceRolesBody body)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            List<VoiceRolesRow> rows = await Database.Data.VoiceRoles.GetRowsAsync(auth.Guild.Id);
            foreach (VoiceRolesRowBody bodyRow in body.Rows)
            {
                VoiceRolesRow row;
                if (rows.Any(x => x.ChannelId == ulong.Parse(bodyRow.ChannelId)))
                    row = rows.First(x => x.ChannelId == ulong.Parse(bodyRow.ChannelId));
                else
                    row = await Database.Data.VoiceRoles.GetRowAsync(auth.Guild.Id, ulong.Parse(bodyRow.ChannelId));

                row.RoleId = ulong.Parse(bodyRow.RoleId);
                await row.SaveAsync();
            }

            foreach (VoiceRolesRow row in rows.Where(x => body.Rows.All(y => ulong.Parse(y.ChannelId) != x.ChannelId)))
                await row.DeleteAsync();

            return new OkResult();
        }
    }

    public class VoiceRolesBody
    {
        public List<VoiceRolesRowBody> Rows { get; set; }

        public VoiceRolesBody(List<VoiceRolesRow> rows)
        {
            Rows = rows.Select(x => new VoiceRolesRowBody(x)).ToList();
        }

        public VoiceRolesBody() { }
    }

    public class VoiceRolesRowBody
    {
        public string ChannelId { get; set; }
        public string RoleId { get; set; }

        public VoiceRolesRowBody(VoiceRolesRow row)
        {
            ChannelId = row.ChannelId.ToString();
            RoleId = row.RoleId.ToString();
        }

        public VoiceRolesRowBody() { }
    }
}
