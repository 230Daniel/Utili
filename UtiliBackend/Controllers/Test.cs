using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UtiliBackend.Controllers
{
    public class Test : Controller
    {
        [HttpGet("dashboard/{guildId}/test")]
        public ActionResult Index([Required] ulong guildId)
        {
            if (!ModelState.IsValid) return new BadRequestResult();
            return new JsonResult(new TestObject(guildId));
        }
    }

    public class TestObject
    {
        public ulong GuildId { get; set; }
        public string Content { get; set; }

        public TestObject(ulong guildId)
        {
            GuildId = guildId;
            Content = "Hello from the other sideeee!";
        }
    }
}
