using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Data;
using Discord;
using Newtonsoft.Json;

namespace UtiliBackend.Controllers.Dashboard
{
    public class JoinMessage : Controller
    {
        [HttpGet("dashboard/{guildId}/joinmessage")]
        public async Task<ActionResult> Get([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            JoinMessageRow row = await Database.Data.JoinMessage.GetRowAsync(auth.Guild.Id);

            return new JsonResult(new JoinMessageBody(row));
        }

        [HttpPost("dashboard/{guildId}/joinmessage")]
        public async Task<ActionResult> Post([Required] ulong guildId, [Required] [FromBody] JoinMessageBody body)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            JoinMessageRow row = await Database.Data.JoinMessage.GetRowAsync(auth.Guild.Id);
            row.Enabled = body.Enabled;
            row.Direct = body.Direct;
            row.ChannelId = ulong.Parse(body.ChannelId);
            row.Title = EString.FromDecoded(body.Title);
            row.Footer = EString.FromDecoded(body.Footer);
            row.Content = EString.FromDecoded(body.Content);
            row.Text = EString.FromDecoded(body.Text);
            row.Image = EString.FromDecoded(body.Image);
            row.Thumbnail = EString.FromDecoded(body.Thumbnail);
            row.Icon = EString.FromDecoded(body.Icon);
            row.Colour = new Color(uint.Parse(body.Colour.Replace("#", ""), System.Globalization.NumberStyles.HexNumber));
            await row.SaveAsync();

            return new OkResult();
        }
    }

    public class JoinMessageBody
    {
        public bool Enabled { get; set; }
        public bool Direct { get; set; }
        public string ChannelId { get; set; }
        public string Title { get; set; }
        public string Footer { get; set; }
        public string Content { get; set; }
        public string Text { get; set; }
        public string Image { get; set; }
        public string Thumbnail { get; set; }
        public string Icon { get; set; }
        public string Colour { get; set; }

        public JoinMessageBody(JoinMessageRow row)
        {
            Enabled = row.Enabled;
            Direct = row.Direct;
            ChannelId = row.ChannelId.ToString();
            Title = row.Title.Value;
            Footer = row.Footer.Value;
            Content = row.Content.Value;
            Text = row.Text.Value;
            Image = row.Image.Value;
            Thumbnail = row.Thumbnail.Value;
            Icon = row.Icon.Value;
            Colour = row.Colour.RawValue.ToString();
        }

        public JoinMessageBody() { }
    }
}
