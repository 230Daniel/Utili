using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace UtiliSite
{
    public static class RedirectHelper
    {
        public static string AddToUrl(ViewContext viewContext, string page)
        {
            return AddToUrl(viewContext.HttpContext.Request.Path, page);
        }

        public static string AddToUrl(string path, string page)
        {
            if (!path.EndsWith("/")) path += "/";
            return path + page;
        }

        public static string ChangeLastPage(ViewContext viewContext, string newPage)
        {
            return ChangeLastPage(viewContext.HttpContext.Request.Path, newPage);
        }

        public static string ChangeLastPage(string path, string newPage)
        {
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);

            int lastPageStart = path.ToCharArray().Select(x => x.ToString()).ToList().LastIndexOf("/");
            path = path.Substring(0, lastPageStart + 1);

            return path + newPage;
        }

        public static string GoToPage(ViewContext viewContext, string page)
        {
            string host = viewContext.HttpContext.Request.Host.ToString();
            if (!host.EndsWith("/")) host += "/";

            return host + page;
        }
    }

    public static class SidebarHelper
    {
        public static string GetIsActive(ViewContext viewContext, string page)
        {
            string path = viewContext.HttpContext.Request.Path;

            if(path.ToLower().EndsWith("/" + page) || path.ToLower().EndsWith("/" + page + "/"))
            {
                return "active";
            }
            return "";
        }
    }
}
