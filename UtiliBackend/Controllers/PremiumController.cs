using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NewDatabase;
using UtiliBackend.Authorisation;

namespace UtiliBackend.Controllers
{
    [Route("premium")]
    public class PremiumController : Controller
    {
        private readonly DatabaseContext _databaseContext;
        
        public PremiumController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        
        [DiscordGuildAuthorise]
        [HttpGet("guild/{GuildId}")]
        public async Task<IActionResult> GuildIsPremiumAsync([Required] ulong guildId)
        {
            return Json(await _databaseContext.PremiumSlots.AnyAsync(x => x.GuildId == guildId));
        }
    }
}
