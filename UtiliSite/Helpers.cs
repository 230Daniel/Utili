using System;
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

            return "https://" + host + page;
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

        public static string GetIsSectionOpen(ViewContext viewContext, string section)
        {
            string path = viewContext.HttpContext.Request.Path;

            string page = path.Split("/").Last().ToLower();

            string[] channelSection =
            {
                "autopurge",
                "channelmirroring",
                "notices"
            };

            string[] messageSection =
            {
                "messagefilter",
                "messagelogs",
                "messagepinning"
            };

            string[] userSection =
            {
                "inactiverole",
                "joinmessage",
                "joinroles",
                "rolepersist"
            };

            string[] voiceSection =
            {
                "voicelink",
                "voiceroles"
            };

            string[] votingSection =
            {
                "votechannels",
                "reputation"
            };

            return section switch
            {
                "channel" => channelSection.Contains(page) ? "show" : "",
                "message" => messageSection.Contains(page) ? "show" : "",
                "user" => userSection.Contains(page) ? "show" : "",
                "voice" => voiceSection.Contains(page) ? "show" : "",
                "voting" => votingSection.Contains(page) ? "show" : "",
                _ => ""
            };
        }
    }

    public static class ContentHelper
    {
        public static string Tooltip(string text, string position = "right")
        {
            text = $"<p>{text.Replace("\n", "</p><p>")}</p>";
            text = text.Replace("~newline~", "\\n");

            string html = $"data-toggle=\"tooltip\" data-placement=\"{position}\" data-html=\"true\" title=\"{text}\"";
            return html;
        }

        public static string ToStandardString(this TimeSpan span)
        {
            string formatted =
                $"{(span.Duration().Days > 0 ? $"{span.Days:00}:" : string.Empty)}{span.Hours:00}:{span.Minutes:00}:{span.Seconds:00}";

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }
    }
}
