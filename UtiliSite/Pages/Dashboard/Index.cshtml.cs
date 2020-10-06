using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Org.BouncyCastle.Asn1.Cmp;
using System.Text.Json;

namespace UtiliSite.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        public string Code { get; set; }

        public void OnGet()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                HttpContext.ChallengeAsync("Discord", new AuthenticationProperties {RedirectUri = "/Dashboard"});
                return;
            }

            Claim userIdClaim = HttpContext.User.FindFirst(x => x.Type.Contains("nameidentifier"));
            ulong userId = ulong.Parse(userIdClaim.Value);

            var user = DiscordModule._client.GetUserAsync(userId).GetAwaiter().GetResult();

            ViewData["text"] = JsonSerializer.Serialize(user, new JsonSerializerOptions{WriteIndented = true});
        }
    }
}
