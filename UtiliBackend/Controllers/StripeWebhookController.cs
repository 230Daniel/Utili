using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace UtiliBackend.Controllers
{
    [Route("stripe")]
    public class StripeWebhookController : Controller
    {
        [IgnoreAntiforgeryToken]
        [HttpPost("webhook")]
        public async Task<IActionResult> WebhookAsync()
        {
            throw new NotImplementedException();
        }
    }
}
