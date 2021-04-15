using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord;

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
            row.Emotes = row.Emotes.Where(x => body.Emotes.Any(y => x.Item1.ToString() == y.Emote))
                .Select(x =>
                {
                    x.Item2 = body.Emotes.First(y => y.Emote == x.Item1.ToString()).Value;
                    return x;
                }).ToList();
            await row.SaveAsync();

            return new OkResult();
        }
    }

    public class ReputationBody
    {
        public List<ReputationEmoteBody> Emotes { get; set; }

        public ReputationBody(ReputationRow row)
        {
            Emotes = row.Emotes.Select(x => new ReputationEmoteBody(x)).ToList();
        }

        public ReputationBody() { }
    }

    public class ReputationEmoteBody
    {
        public string Emote { get; set; }
        public int Value { get; set; }

        public ReputationEmoteBody((string, int) obj)
        {
            Emote = obj.Item1;
            Value = obj.Item2;
        }

        public ReputationEmoteBody() { }
    }
}