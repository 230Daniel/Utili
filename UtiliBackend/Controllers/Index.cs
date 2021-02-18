using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
