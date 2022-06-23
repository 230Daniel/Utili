using System.Xml;
using Utili.Database.Entities;

namespace Utili.Backend.Models
{
    public class AutopurgeConfigurationModel
    {
        public string ChannelId { get; set; }
        public string Timespan { get; set; }
        public int Mode { get; set; }

        public void ApplyTo(AutopurgeConfiguration configuration)
        {
            configuration.Timespan = XmlConvert.ToTimeSpan(Timespan);
            configuration.Mode = (AutopurgeMode)Mode;
        }
    }
}
