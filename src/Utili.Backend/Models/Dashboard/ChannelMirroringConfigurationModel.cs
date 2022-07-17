using Utili.Database.Entities;

namespace Utili.Backend.Models;

public class ChannelMirroringConfigurationModel
{
    public string ChannelId { get; set; }
    public string DestinationChannelId { get; set; }
    public int AuthorDisplayMode { get; set; }

    public void ApplyTo(ChannelMirroringConfiguration configuration)
    {
        configuration.DestinationChannelId = ulong.Parse(DestinationChannelId);
        configuration.AuthorDisplayMode = (ChannelMirroringAuthorDisplayMode)AuthorDisplayMode;
    }
}