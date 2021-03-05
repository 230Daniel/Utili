using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace UtiliBackend.Controllers
{
    public class Test : Controller
    {
        [HttpGet("dashboard/{guildId}/test")]
        public async Task<ActionResult> Index([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            return new JsonResult(auth.Guild.Name);
        }
    }
}
