using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

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
    }

    public class PremiumGuildBody
    {
        public bool Premium { get; set; }

        public PremiumGuildBody(bool premium)
        {
            Premium = premium;
        }
    }
}
