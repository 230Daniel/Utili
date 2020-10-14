using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages
{
    public class ReturnModel : PageModel
    {
        public void OnGet()
        {
            if (HttpContext.Request.Query.ContainsKey("error"))
            {
                HttpContext.Response.Redirect($"https://{HttpContext.Request.Host}/dashboard");
                return;
            }

            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);
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
