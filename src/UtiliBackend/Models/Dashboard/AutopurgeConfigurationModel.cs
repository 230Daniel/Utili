using System.Xml;
using Database.Entities;

namespace UtiliBackend.Models
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
