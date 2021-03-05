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
    public class MessageFilter : Controller
    {
        [HttpGet("dashboard/{guildId}/messagefilter")]
        public async Task<ActionResult> Get([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            List<MessageFilterRow> rows = await Database.Data.MessageFilter.GetRowsAsync(auth.Guild.Id);
            return new JsonResult(new MessageFilterBody(rows));
        }

        [HttpPost("dashboard/{guildId}/messagefilter")]
        public async Task<ActionResult> Post([Required] ulong guildId, [Required] [FromBody] MessageFilterBody body)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            List<MessageFilterRow> rows = await Database.Data.MessageFilter.GetRowsAsync(auth.Guild.Id);
            foreach (MessageFilterRowBody bodyRow in body.Rows)
            {
                MessageFilterRow row;
                if (rows.Any(x => x.ChannelId == ulong.Parse(bodyRow.ChannelId)))
                    row = rows.First(x => x.ChannelId == ulong.Parse(bodyRow.ChannelId));
                else
                    row = await Database.Data.MessageFilter.GetRowAsync(auth.Guild.Id, ulong.Parse(bodyRow.ChannelId));

                row.Mode = bodyRow.Mode;
                row.Complex = EString.FromDecoded(bodyRow.Complex);
                await row.SaveAsync();
            }

            foreach (MessageFilterRow row in rows.Where(x => body.Rows.All(y => ulong.Parse(y.ChannelId) != x.ChannelId)))
                await row.DeleteAsync();

            return new OkResult();
        }
    }

    public class MessageFilterBody
    {
        public List<MessageFilterRowBody> Rows { get; set; }

        public MessageFilterBody(List<MessageFilterRow> rows)
        {
            Rows = rows.Select(x => new MessageFilterRowBody(x)).ToList();
        }

        public MessageFilterBody() { }
    }

    public class MessageFilterRowBody
    {
        public string ChannelId { get; set; }
        public int Mode { get; set; }
        public string Complex { get; set; }

        public MessageFilterRowBody(MessageFilterRow row)
        {
            ChannelId = row.ChannelId.ToString();
            Mode = row.Mode;
            Complex = row.Complex.Value;
        }

        public MessageFilterRowBody() { }
    }
}
