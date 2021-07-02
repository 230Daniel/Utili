using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Database.Data;

namespace UtiliBackend.Controllers.Dashboard
{
    public class Autopurge : Controller
    {
        [HttpGet("dashboard/{guildId}/autopurge")]
        public async Task<ActionResult> Get([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            List<AutopurgeRow> rows = await Database.Data.Autopurge.GetRowsAsync(auth.Guild.Id);
            return new JsonResult(new AutopurgeBody(rows));
        }

        [HttpPost("dashboard/{guildId}/autopurge")]
        public async Task<ActionResult> Post([Required] ulong guildId, [Required] [FromBody] AutopurgeBody body)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            List<AutopurgeRow> rows = await Database.Data.Autopurge.GetRowsAsync(auth.Guild.Id);
            foreach (AutopurgeRowBody bodyRow in body.Rows)
            {
                AutopurgeRow row;
                if (rows.Any(x => x.ChannelId == ulong.Parse(bodyRow.ChannelId)))
                    row = rows.First(x => x.ChannelId == ulong.Parse(bodyRow.ChannelId));
                else
                    row = await Database.Data.Autopurge.GetRowAsync(auth.Guild.Id, ulong.Parse(bodyRow.ChannelId));

                bool isNew = row.Mode == 2 && bodyRow.Mode != 2;

                row.Timespan = XmlConvert.ToTimeSpan(bodyRow.Timespan);
                row.Mode = bodyRow.Mode;
                
                await row.SaveAsync();

                if (isNew)
                {
                    MiscRow miscRow = new(auth.Guild.Id, "RequiresAutopurgeMessageDownload", row.ChannelId.ToString());
                    await miscRow.SaveAsync();
                }
            }

            foreach (AutopurgeRow row in rows.Where(x => body.Rows.All(y => ulong.Parse(y.ChannelId) != x.ChannelId)))
                await row.DeleteAsync();

            return new OkResult();
        }
    }

    public class AutopurgeBody
    {
        public List<AutopurgeRowBody> Rows { get; set; }

        public AutopurgeBody(List<AutopurgeRow> rows)
        {
            Rows = rows.Select(x => new AutopurgeRowBody(x)).ToList();
        }

        public AutopurgeBody() { }
    }

    public class AutopurgeRowBody
    {
        public string ChannelId { get; set; }
        public string Timespan { get; set; }
        public int Mode { get; set; }

        public AutopurgeRowBody(AutopurgeRow row)
        {
            ChannelId = row.ChannelId.ToString();
            Timespan = XmlConvert.ToString(row.Timespan);
            Mode = row.Mode;
        }

        public AutopurgeRowBody() { }
    }
}
