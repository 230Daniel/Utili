using NewDatabase.Entities;

namespace UtiliBackend.Models
{
    public class ChannelMirroringConfigurationModel
    {
        public string ChannelId { get; set; }
        public string DestinationChannelId { get; set; }

        public void ApplyTo(ChannelMirroringConfiguration configuration)
        {
            configuration.DestinationChannelId = ulong.Parse(DestinationChannelId);
        }
    }
}
