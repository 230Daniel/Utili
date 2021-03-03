using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Database;
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
            row.DeletedChannelId = body.DeletedChannelId;
            row.EditedChannelId = body.EditedChannelId;
            row.ExcludedChannels = body.ExcludedChannels;
            await row.SaveAsync();

            return new OkResult();
        }
    }

    public class MessageLogsBody
    {
        public ulong DeletedChannelId { get; set; }
        public ulong EditedChannelId { get; set; }
        public List<ulong> ExcludedChannels { get; set; }

        public MessageLogsBody(MessageLogsRow row)
        {
            DeletedChannelId = row.DeletedChannelId;
            EditedChannelId = row.EditedChannelId;
            ExcludedChannels = row.ExcludedChannels;
        }

        public MessageLogsBody() { }
    }
}