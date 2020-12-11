using System;
using System.Collections.Generic;
using System.Linq;
using Database.Data;
using Discord;
using Discord.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UtiliSite.Pages.Dashboard
{
    public class VoteChannelsModel : PageModel
    {
        public void OnGet()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);
            if(!auth.Authenticated) return;
            ViewData["user"] = auth.User;
            ViewData["guild"] = auth.Guild;
            ViewData["premium"] = Database.Premium.IsPremium(auth.Guild.Id);

            List<VoteChannelsRow> rows = VoteChannels.GetRows(auth.Guild.Id);
            ViewData["rows"] = rows;

            List<RestTextChannel> channels = DiscordModule.GetTextChannelsAsync(auth.Guild).GetAwaiter().GetResult();
            ViewData["channels"] = channels;

            List<RestTextChannel> nonVoteChannels = channels.Where(x => rows.All(y => y.ChannelId != x.Id)).ToList();
            ViewData["nonVoteChannels"] = nonVoteChannels;
        }

        public void OnPost()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);
            int mode = int.Parse(HttpContext.Request.Form["mode"]);

            VoteChannelsRow row = VoteChannels.GetRows(auth.Guild.Id, channelId).First();
            row.Mode = mode;
            VoteChannels.SaveRow(row);

            HttpContext.Response.StatusCode = 200;
        }

        public void OnPostAdd()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);
            RestTextChannel channel = auth.Guild.GetTextChannelAsync(channelId).GetAwaiter().GetResult();

            VoteChannelsRow newRow = new VoteChannelsRow
            {
                GuildId = auth.Guild.Id,
                ChannelId = channel.Id,
                Mode = 0,
                Emotes = new List<IEmote>()
            };
            VoteChannels.SaveRow(newRow);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public void OnPostRemove()
        {
            AuthDetails auth = Auth.GetAuthDetails(HttpContext, HttpContext.Request.Path);

            if (!auth.Authenticated)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            ulong channelId = ulong.Parse(HttpContext.Request.Form["channel"]);

            VoteChannelsRow row = VoteChannels.GetRows(auth.Guild.Id, channelId).First();
            VoteChannels.DeleteRow(row);

            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.Redirect(HttpContext.Request.Path);
        }

        public static string GetIsSelected(int mode, VoteChannelsRow row)
        {
            if (row.Mode == mode) return "selected";
            return "";
        }
    }
}
