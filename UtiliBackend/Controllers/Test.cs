using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;
using UtiliBackend.Controllers;

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
