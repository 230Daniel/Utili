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
    public class VoiceLink : Controller
    {
        [HttpGet("dashboard/{guildId}/voicelink")]
        public async Task<ActionResult> Get([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            VoiceLinkRow row = await Database.Data.VoiceLink.GetRowAsync(auth.Guild.Id);
            return new JsonResult(new VoiceLinkBody(row));
        }

        [HttpPost("dashboard/{guildId}/voicelink")]
        public async Task<ActionResult> Post([Required] ulong guildId, [Required] [FromBody] VoiceLinkBody body)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            VoiceLinkRow row = await Database.Data.VoiceLink.GetRowAsync(auth.Guild.Id);
            row.Enabled = body.Enabled;
            row.DeleteChannels = body.DeleteChannels;
            row.Prefix = EString.FromDecoded(body.Prefix);
            row.ExcludedChannels = body.ExcludedChannels;
            await row.SaveAsync();

            return new OkResult();
        }
    }

    public class VoiceLinkBody
    {
        public bool Enabled { get; set; }
        public bool DeleteChannels { get; set; }
        public List<ulong> ExcludedChannels { get; set; }
        public string Prefix { get; set; }

        public VoiceLinkBody(VoiceLinkRow row)
        {
            Enabled = row.Enabled;
            DeleteChannels = row.DeleteChannels;
            ExcludedChannels = row.ExcludedChannels;
            Prefix = row.Prefix.Value;
        }
    }
}
