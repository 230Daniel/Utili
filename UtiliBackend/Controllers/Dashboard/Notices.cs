using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Database;
using Database.Data;
using Discord;

namespace UtiliBackend.Controllers.Dashboard
{
    public class Notices : Controller
    {
        [HttpGet("dashboard/{guildId}/notices")]
        public async Task<ActionResult> Get([Required] ulong guildId)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            List<NoticesRow> rows = await Database.Data.Notices.GetRowsAsync(auth.Guild.Id);
            return new JsonResult(new NoticesBody(rows));
        }

        [HttpPost("dashboard/{guildId}/notices")]
        public async Task<ActionResult> Post([Required] ulong guildId, [Required] [FromBody] NoticesBody body)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext, guildId);
            if (!auth.Authorised) return auth.Action;

            List<NoticesRow> rows = await Database.Data.Notices.GetRowsAsync(auth.Guild.Id);
            foreach (NoticesRowBody bodyRow in body.Rows)
            {
                NoticesRow row;
                if (rows.Any(x => x.ChannelId == ulong.Parse(bodyRow.ChannelId)))
                    row = rows.First(x => x.ChannelId == ulong.Parse(bodyRow.ChannelId));
                else
                    row = await Database.Data.Notices.GetRowAsync(auth.Guild.Id, ulong.Parse(bodyRow.ChannelId));

                row.Enabled = bodyRow.Enabled;
                row.Delay = XmlConvert.ToTimeSpan(bodyRow.Delay);
                row.Title = EString.FromDecoded(bodyRow.Title);
                row.Footer = EString.FromDecoded(bodyRow.Footer);
                row.Content = EString.FromDecoded(bodyRow.Content);
                row.Text = EString.FromDecoded(bodyRow.Text);
                row.Image = EString.FromDecoded(bodyRow.Image);
                row.Thumbnail = EString.FromDecoded(bodyRow.Thumbnail);
                row.Icon = EString.FromDecoded(bodyRow.Icon);
                row.Colour = uint.Parse(bodyRow.Colour.Replace("#", ""), System.Globalization.NumberStyles.HexNumber);

                bool isNew = row.New;
                await row.SaveAsync();

                if (bodyRow.Changed || isNew)
                {
                    MiscRow miscRow = new MiscRow(auth.Guild.Id, "RequiresNoticeUpdate", row.ChannelId.ToString());
                    _ = Misc.SaveRowAsync(miscRow);
                }
            }

            foreach (NoticesRow row in rows.Where(x => body.Rows.All(y => ulong.Parse(y.ChannelId) != x.ChannelId)))
                await row.DeleteAsync();

            return new OkResult();
        }
    }

    public class NoticesBody
    {
        public List<NoticesRowBody> Rows { get; set; }

        public NoticesBody(List<NoticesRow> rows)
        {
            Rows = rows.Select(x => new NoticesRowBody(x)).ToList();
        }

        public NoticesBody() { }
    }

    public class NoticesRowBody
    {
        public string ChannelId { get; set; }
        public bool Enabled { get; set; }
        public string Delay { get; set; }
        public string Title { get; set; }
        public string Footer { get; set; }
        public string Content { get; set; }
        public string Text { get; set; }
        public string Image { get; set; }
        public string Thumbnail { get; set; }
        public string Icon { get; set; }
        public string Colour { get; set; }
        public bool Changed { get; set; }
        
        public NoticesRowBody(NoticesRow row)
        {
            ChannelId = row.ChannelId.ToString();
            Enabled = row.Enabled;
            Delay = XmlConvert.ToString(row.Delay);
            Title = row.Title.Value;
            Footer = row.Footer.Value;
            Content = row.Content.Value;
            Text = row.Text.Value;
            Image = row.Image.Value;
            Thumbnail = row.Thumbnail.Value;
            Icon = row.Icon.Value;
            Colour = row.Colour.ToString("X6");
            Changed = false;
        }

        public NoticesRowBody() { }
    }
}
