namespace UtiliBackend.Models
{
    public class JoinMessageConfigurationModel
    {
        public bool Enabled { get; set; }
        public int Mode { get; set; }
        public string ChannelId { get; set; }
        public string Title { get; set; }
        public string Footer { get; set; }
        public string Content { get; set; }
        public string Text { get; set; }
        public string Image { get; set; }
        public string Thumbnail { get; set; }
        public string Icon { get; set; }
        public string Colour { get; set; }
    }
}
