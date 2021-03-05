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
    public class VoteChannels : Controller
    {
        [HttpGet("dashboard/{guildId}/votechannels")]
        public async Task<ActionResult> Get([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            List<VoteChannelsRow> rows = await Database.Data.VoteChannels.GetRowsAsync(auth.Guild.Id);
            return new JsonResult(new VoteChannelsBody(rows));
        }

        [HttpPost("dashboard/{guildId}/votechannels")]
        public async Task<ActionResult> Post([Required] ulong guildId, [Required] [FromBody] VoteChannelsBody body)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            List<VoteChannelsRow> rows = await Database.Data.VoteChannels.GetRowsAsync(auth.Guild.Id);
            foreach (VoteChannelsRowBody bodyRow in body.Rows)
            {
                VoteChannelsRow row;
                if (rows.Any(x => x.ChannelId == ulong.Parse(bodyRow.ChannelId)))
                    row = rows.First(x => x.ChannelId == ulong.Parse(bodyRow.ChannelId));
                else
                    row = await Database.Data.VoteChannels.GetRowAsync(auth.Guild.Id, ulong.Parse(bodyRow.ChannelId));

                row.Mode = bodyRow.Mode;
                row.Emotes = row.Emotes.Where(x => bodyRow.Emotes.Any(y => y == x.ToString())).ToList();
                await row.SaveAsync();
            }

            foreach (VoteChannelsRow row in rows.Where(x => body.Rows.All(y => ulong.Parse(y.ChannelId) != x.ChannelId)))
                await row.DeleteAsync();

            return new OkResult();
        }
    }

    public class VoteChannelsBody
    {
        public List<VoteChannelsRowBody> Rows { get; set; }

        public VoteChannelsBody(List<VoteChannelsRow> rows)
        {
            Rows = rows.Select(x => new VoteChannelsRowBody(x)).ToList();
        }

        public VoteChannelsBody() { }
    }

    public class VoteChannelsRowBody
    {
        public string ChannelId { get; set; }
        public int Mode { get; set; }
        public List<string> Emotes { get; set; }

        public VoteChannelsRowBody(VoteChannelsRow row)
        {
            ChannelId = row.ChannelId.ToString();
            Mode = row.Mode;
            Emotes = row.Emotes.Select(x => x.ToString()).ToList();
        }

        public VoteChannelsRowBody() { }
    }
}
