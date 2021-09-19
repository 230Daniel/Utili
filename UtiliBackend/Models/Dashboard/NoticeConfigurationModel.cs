using System.Globalization;
using System.Xml;
using NewDatabase.Entities;

namespace UtiliBackend.Models
{
    public class NoticeConfigurationModel
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

        public void ApplyTo(NoticeConfiguration configuration)
        {
            configuration.Enabled = Enabled;
            configuration.Delay = XmlConvert.ToTimeSpan(Delay);
            configuration.Title = Title;
            configuration.Footer = Footer;
            configuration.Content = Content;
            configuration.Text = Text;
            configuration.Image = Image;
            configuration.Thumbnail = Thumbnail;
            configuration.Icon = Icon;
            configuration.Colour = uint.Parse(Colour.Replace("#", ""), NumberStyles.HexNumber);
            
            // Don't set this to false from the web side
            // The bot will set it to false once it updates the notice
            if(Changed) configuration.UpdatedFromDashboard = true;
        }
    }
}
