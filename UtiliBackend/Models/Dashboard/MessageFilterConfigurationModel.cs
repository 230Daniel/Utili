using NewDatabase.Entities;

namespace UtiliBackend.Models
{
    public class MessageFilterConfigurationModel
    {
        public string ChannelId { get; set; }
        public int Mode { get; set; }
        public string RegEx { get; set; }

        public void ApplyTo(MessageFilterConfiguration configuration)
        {
            configuration.Mode = (MessageFilterMode) Mode;
            configuration.RegEx = RegEx;
        }
    }
}
