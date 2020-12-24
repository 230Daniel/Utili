using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages
{
    public class ReturnModel : PageModel
    {
        public async Task OnGet()
        {
            if (HttpContext.Request.Query.ContainsKey("error"))
            {
                HttpContext.Response.Redirect($"https://{HttpContext.Request.Host}/dashboard");
                return;
            }

            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if(!auth.Authenticated) return;

            string url = GetRedirect(auth.User.Id, HttpContext);
            HttpContext.Response.Redirect(url);
        }

        private static Dictionary<ulong, string> _redirects = new Dictionary<ulong, string>();

        public static string GetRedirect(ulong userId, HttpContext httpContext)
        {
            if (_redirects.TryGetValue(userId, out string url))
            {
                _redirects.Remove(userId);
                return url;
            }

            return $"https://{httpContext.Request.Host}/dashboard";
        }

        public static void SaveRedirect(ulong userId, string url)
        {
            if (_redirects.ContainsKey(userId))
            {
                _redirects.Remove(userId);
            }

            _redirects.Add(userId, url);
        }
    }
}
