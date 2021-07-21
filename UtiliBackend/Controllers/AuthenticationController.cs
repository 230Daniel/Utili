using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UtiliBackend.Authorisation;
using UtiliBackend.Extensions;
using UtiliBackend.Models;

namespace UtiliBackend.Controllers
{
    [Route("authentication")]
    public class AuthenticationController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IAntiforgery _antiforgery;

        public AuthenticationController(IConfiguration configuration, IAntiforgery antiforgery)
        {
            _configuration = configuration;
            _antiforgery = antiforgery;
        }

        [HttpGet("signin")]
        public async Task<IActionResult> SignInAsync()
        {
            AuthenticationProperties authProperties = new()
            {
                AllowRefresh = true,
                IsPersistent = true,
                RedirectUri = $"{_configuration["Frontend:Origin"]}/return"
            };
            
            return Challenge(authProperties, "Discord");
        }
        
        [HttpPost("signout")]
        public new IActionResult SignOut()
        {
            return SignOut("Cookies");
        }
        
        [DiscordAuthorise]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var user = HttpContext.GetDiscordUser();
            return Json(new AuthenticationInfoModel()
            {
                Username = user.Username,
                AvatarUrl = user.GetAvatarUrl()
            });
        }
        
        [IgnoreAntiforgeryToken]
        [HttpGet("antiforgery")]
        public async Task<IActionResult> AntiforgeryAsync()
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            return Json(tokens.RequestToken);
        }
    }
}
