using Utili.Database.Entities;

namespace Utili.Backend.Models;

public class MessageFilterConfigurationModel
{
    public string ChannelId { get; set; }
    public int Mode { get; set; }
    public string RegEx { get; set; }
    public string DeletionMessage { get; set; }
    public bool EnforceInThreads { get; set; }

    public void ApplyTo(MessageFilterConfiguration configuration)
    {
        configuration.Mode = (MessageFilterMode)Mode;
        configuration.RegEx = RegEx;
        configuration.DeletionMessage = DeletionMessage;
        configuration.EnforceInThreads = EnforceInThreads;
    }
}