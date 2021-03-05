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
    public class Reputation : Controller
    {
        [HttpGet("dashboard/{guildId}/reputation")]
        public async Task<ActionResult> Get([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            ReputationRow row = await Database.Data.Reputation.GetRowAsync(auth.Guild.Id);
            return new JsonResult(new ReputationBody(row));
        }

        [HttpPost("dashboard/{guildId}/reputation")]
        public async Task<ActionResult> Post([Required] ulong guildId, [Required] [FromBody] ReputationBody body)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            ReputationRow row = await Database.Data.Reputation.GetRowAsync(auth.Guild.Id);
            row.Emotes = row.Emotes.Where(x => body.Emotes.Any(y => x.Item1.ToString() == y.Item1))
                .Select(x =>
                {
                    x.Item2 = body.Emotes.First(y => y.Item1 == x.Item1.ToString()).Item2;
                    return x;
                }).ToList();
            await row.SaveAsync();

            return new OkResult();
        }
    }

    public class ReputationBody
    {
        public List<(string, int)> Emotes { get; set; }

        public ReputationBody(ReputationRow row)
        {
            Emotes = row.Emotes.Select(x => (x.Item1.ToString(), x.Item2)).ToList();
        }

        public ReputationBody() { }
    }
}