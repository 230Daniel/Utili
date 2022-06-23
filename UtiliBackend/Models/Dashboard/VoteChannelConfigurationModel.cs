using System.Collections.Generic;
using Database.Entities;

namespace UtiliBackend.Models
{
    public class VoteChannelConfigurationModel
    {
        public string ChannelId { get; set; }
        public int Mode { get; set; }
        public List<string> Emojis { get; set; }

        public void ApplyTo(VoteChannelConfiguration configuration)
        {
            configuration.Mode = (VoteChannelMode)Mode;
            configuration.Emojis = Emojis;
        }
    }
}
