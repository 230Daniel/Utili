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
    public class ChannelMirroring : Controller
    {
        [HttpGet("dashboard/{guildId}/channelmirroring")]
        public async Task<ActionResult> Get([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            List<ChannelMirroringRow> rows = await Database.Data.ChannelMirroring.GetRowsAsync(auth.Guild.Id);
            return new JsonResult(new ChannelMirroringBody(rows));
        }

        [HttpPost("dashboard/{guildId}/channelmirroring")]
        public async Task<ActionResult> Post([Required] ulong guildId, [Required] [FromBody] ChannelMirroringBody body)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            List<ChannelMirroringRow> rows = await Database.Data.ChannelMirroring.GetRowsAsync(auth.Guild.Id);
            foreach (ChannelMirroringRowBody bodyRow in body.Rows)
            {
                ChannelMirroringRow row;
                if (rows.Any(x => x.FromChannelId == ulong.Parse(bodyRow.FromChannelId)))
                    row = rows.First(x => x.FromChannelId == ulong.Parse(bodyRow.FromChannelId));
                else
                    row = await Database.Data.ChannelMirroring.GetRowAsync(auth.Guild.Id, ulong.Parse(bodyRow.FromChannelId));

                row.ToChannelId = ulong.Parse(bodyRow.ToChannelId);
                await row.SaveAsync();
            }

            foreach (ChannelMirroringRow row in rows.Where(x => body.Rows.All(y => ulong.Parse(y.FromChannelId) != x.FromChannelId)))
                await row.DeleteAsync();

            return new OkResult();
        }
    }

    public class ChannelMirroringBody
    {
        public List<ChannelMirroringRowBody> Rows { get; set; }

        public ChannelMirroringBody(List<ChannelMirroringRow> rows)
        {
            Rows = rows.Select(x => new ChannelMirroringRowBody(x)).ToList();
        }

        public ChannelMirroringBody() { }
    }

    public class ChannelMirroringRowBody
    {
        public string FromChannelId { get; set; }
        public string ToChannelId { get; set; }

        public ChannelMirroringRowBody(ChannelMirroringRow row)
        {
            FromChannelId = row.FromChannelId.ToString();
            ToChannelId = row.ToChannelId.ToString();
        }

        public ChannelMirroringRowBody() { }
    }
}
