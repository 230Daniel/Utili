using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages
{
    public class ReturnModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if(!auth.Authenticated) return auth.Action;

            string url = GetRedirect(auth.User.Id, HttpContext);

            if (HttpContext.Request.Query.ContainsKey("error"))
            {
                return url.Contains("dashboard") ? new RedirectResult($"https://{HttpContext.Request.Host}/dashboard") : new RedirectResult($"https://{HttpContext.Request.Host}");
            }

            return new RedirectResult(url);
        }

        private static Dictionary<ulong, string> _redirects = new Dictionary<ulong, string>();

        private static string GetRedirect(ulong userId, HttpContext httpContext)
        {
            lock (_redirects)
            {
                if (_redirects.TryGetValue(userId, out string url))
                {
                    _redirects.Remove(userId);
                    return url;
                }
            }

            return $"https://{httpContext.Request.Host}/";
        }

        public static void SaveRedirect(ulong userId, string url)
        {
            lock (_redirects)
            {
                if (_redirects.ContainsKey(userId))
                {
                    _redirects.Remove(userId);
                }

                _redirects.Add(userId, url);
            }
        }
    }
}
