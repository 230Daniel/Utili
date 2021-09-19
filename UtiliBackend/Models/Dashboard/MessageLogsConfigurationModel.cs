using System.Collections.Generic;
using System.Linq;
using NewDatabase.Entities;

namespace UtiliBackend.Models
{
    public class MessageLogsConfigurationModel
    {
        public string DeletedChannelId { get; set; }
        public string EditedChannelId { get; set; }
        public List<string> ExcludedChannels { get; set; }

        public void ApplyTo(MessageLogsConfiguration configuration)
        {
            configuration.DeletedChannelId = ulong.Parse(DeletedChannelId);
            configuration.EditedChannelId = ulong.Parse(EditedChannelId);
            configuration.ExcludedChannels = ExcludedChannels.Select(ulong.Parse).ToList();
        }
    }
}
