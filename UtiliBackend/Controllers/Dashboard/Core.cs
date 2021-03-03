using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Data;
using Newtonsoft.Json;

namespace UtiliBackend.Controllers.Dashboard
{
    public class Core : Controller
    {
        [HttpGet("dashboard/{guildId}/core")]
        public async Task<ActionResult> Get([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            CoreRow row = await Database.Data.Core.GetRowAsync(auth.Guild.Id);
            string nickname = await DiscordModule.GetBotNicknameAsync(auth.Guild.Id);

            return new JsonResult(new CoreBody(row, nickname));
        }

        [HttpPost("dashboard/{guildId}/core")]
        public async Task<ActionResult> Post([Required] ulong guildId, [Required] [FromBody] CoreBody body)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            await DiscordModule.SetNicknameAsync(auth.Guild.Id, body.Nickname);

            CoreRow row = await Database.Data.Core.GetRowAsync(auth.Guild.Id);
            row.Prefix = EString.FromDecoded(body.Prefix);
            row.EnableCommands = body.EnableCommands;
            row.ExcludedChannels = body.ExcludedChannels.Select(ulong.Parse).ToList();
            await row.SaveAsync();

            return new OkResult();
        }
    }

    public class CoreBody
    {
        public string Nickname { get; set; }
        public string Prefix { get; set; }
        public bool EnableCommands { get; set; }
        public List<string> ExcludedChannels { get; set; }

        public CoreBody(CoreRow row, string nickname)
        {
            Nickname = nickname;
            Prefix = row.Prefix.Value;
            EnableCommands = row.EnableCommands;
            ExcludedChannels = row.ExcludedChannels.Select(x => x.ToString()).ToList();
        }

        public CoreBody() { }
    }
}
