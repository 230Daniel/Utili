using Microsoft.AspNetCore.Mvc;

namespace UtiliBackend.Controllers
{
    public class Index : Controller
    {
        [HttpGet("")]
        public ActionResult Redirect()
        {
            return new RedirectResult(Main.Config.Frontend);
        }

        [HttpGet("status")]
        public ActionResult Status()
        {
            return new StatusCodeResult(200);
        }
    }
}
