using System.Collections.Generic;
using System.Linq;
using Utili.Database.Entities;

namespace Utili.Backend.Models;

public class VoiceLinkConfigurationModel
{
    public bool Enabled { get; set; }
    public bool DeleteChannels { get; set; }
    public string ChannelPrefix { get; set; }
    public List<string> ExcludedChannels { get; set; }

    public void ApplyTo(VoiceLinkConfiguration configuration)
    {
        configuration.Enabled = Enabled;
        configuration.DeleteChannels = DeleteChannels;
        configuration.ChannelPrefix = ChannelPrefix;
        configuration.ExcludedChannels = ExcludedChannels.Select(ulong.Parse).ToList();
    }
}