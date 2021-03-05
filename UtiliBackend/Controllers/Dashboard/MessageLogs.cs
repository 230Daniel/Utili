using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;

namespace UtiliBackend.Controllers.Dashboard
{
    public class MessageLogs : Controller
    {
        [HttpGet("dashboard/{guildId}/messagelogs")]
        public async Task<ActionResult> Get([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            MessageLogsRow row = await Database.Data.MessageLogs.GetRowAsync(auth.Guild.Id);
            return new JsonResult(new MessageLogsBody(row));
        }

        [HttpPost("dashboard/{guildId}/messagelogs")]
        public async Task<ActionResult> Post([Required] ulong guildId, [Required] [FromBody] MessageLogsBody body)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            MessageLogsRow row = await Database.Data.MessageLogs.GetRowAsync(auth.Guild.Id);
            row.DeletedChannelId = ulong.Parse(body.DeletedChannelId);
            row.EditedChannelId = ulong.Parse(body.EditedChannelId);
            row.ExcludedChannels = body.ExcludedChannels.Select(ulong.Parse).ToList();
            await row.SaveAsync();

            return new OkResult();
        }
    }

    public class MessageLogsBody
    {
        public string DeletedChannelId { get; set; }
        public string EditedChannelId { get; set; }
        public List<string> ExcludedChannels { get; set; }

        public MessageLogsBody(MessageLogsRow row)
        {
            DeletedChannelId = row.DeletedChannelId.ToString();
            EditedChannelId = row.EditedChannelId.ToString();
            ExcludedChannels = row.ExcludedChannels.Select(x => x.ToString()).ToList();
        }

        public MessageLogsBody() { }
    }
}