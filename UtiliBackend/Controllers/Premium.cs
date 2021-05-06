using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord.Rest;

namespace UtiliBackend.Controllers
{
    public class Premium : Controller
    {
        [HttpGet("premium/guild/{guildId}")]
        public async Task<ActionResult> Guild([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            PremiumGuildBody body = new PremiumGuildBody(await Database.Data.Premium.IsGuildPremiumAsync(auth.Guild.Id));
            return new JsonResult(body);
        }

        [HttpGet("premium/slots")]
        public async Task<ActionResult> Slots()
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext);
            if (!auth.Authorised) return auth.Action;

            List<PremiumRow> rows = await Database.Data.Premium.GetUserRowsAsync(auth.User.Id);
            return new JsonResult(new PremiumSlotsBody(rows));
        }

        [HttpPost("premium/slots")]
        public async Task<ActionResult> Slots([Required] [FromBody] PremiumSlotsBody slotsBody)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext);
            if (!auth.Authorised) return auth.Action;

            List<PremiumRow> rows = await Database.Data.Premium.GetUserRowsAsync(auth.User.Id);
            foreach (PremiumRow row in rows)
            {
                if (slotsBody.Slots.Any(x => x.SlotId == row.SlotId))
                {
                    ulong before = row.GuildId;
                    row.GuildId = ulong.Parse(slotsBody.Slots.First(x => x.SlotId == row.SlotId).GuildId);
                    if (row.GuildId != before)
                        await row.SaveAsync();
                }
            }

            return new OkResult();
        }

        [HttpGet("premium/guilds")]
        public async Task<IActionResult> Guilds()
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext);
            if (!auth.Authorised) return auth.Action;

            List<RestUserGuild> mutualGuilds = await DiscordModule.GetGuildsAsync(auth.Client);
            return new JsonResult(mutualGuilds.Select(x => new PremiumGuild(x)));
        }
    }

    public class PremiumGuildBody
    {
        public bool Premium { get; set; }

        public PremiumGuildBody(bool premium)
        {
            Premium = premium;
        }
    }

    public class PremiumSlotsBody
    {
        public List<PremiumSlotBody> Slots { get; set; }

        public PremiumSlotsBody(List<PremiumRow> rows)
        {
            Slots = rows.Select(x => new PremiumSlotBody(x)).ToList();
        }

        public PremiumSlotsBody() { }
    }

    public class PremiumSlotBody
    {
        public int SlotId { get; set; }
        public string GuildId { get; set; }

        public PremiumSlotBody(PremiumRow row)
        {
            SlotId = row.SlotId;
            GuildId = row.GuildId.ToString();
        }

        public PremiumSlotBody() { }
    }

    public class PremiumGuild
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public PremiumGuild(RestUserGuild guild)
        {
            Id = guild.Id.ToString();
            Name = guild.Name;
        }
    }
}
