using System.Globalization;
using Utili.Database.Entities;

namespace Utili.Backend.Models
{
    public class JoinMessageConfigurationModel
    {
        public bool Enabled { get; set; }
        public int Mode { get; set; }
        public string ChannelId { get; set; }
        public bool CreateThread { get; set; }
        public string ThreadTitle { get; set; }
        public string Title { get; set; }
        public string Footer { get; set; }
        public string Content { get; set; }
        public string Text { get; set; }
        public string Image { get; set; }
        public string Thumbnail { get; set; }
        public string Icon { get; set; }
        public string Colour { get; set; }

        public void ApplyTo(JoinMessageConfiguration configuration)
        {
            configuration.Enabled = Enabled;
            configuration.Mode = (JoinMessageMode)Mode;
            configuration.ChannelId = ulong.Parse(ChannelId);
            configuration.CreateThread = CreateThread;
            configuration.ThreadTitle = ThreadTitle;
            configuration.Title = Title;
            configuration.Footer = Footer;
            configuration.Content = Content;
            configuration.Text = Text;
            configuration.Image = Image;
            configuration.Thumbnail = Thumbnail;
            configuration.Icon = Icon;
            configuration.Colour = uint.Parse(Colour.Replace("#", ""), NumberStyles.HexNumber);
        }
    }
}
