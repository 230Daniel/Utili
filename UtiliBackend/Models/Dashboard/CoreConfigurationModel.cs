using System.Collections.Generic;
using System.Linq;
using NewDatabase.Entities;

namespace UtiliBackend.Models
{
    public class CoreConfigurationModel
    {
        public string Prefix { get; set; }
        public bool CommandsEnabled { get; set; }
        public List<string> NonCommandChannels { get; set; }

        public void ApplyTo(CoreConfiguration configuration)
        {
            configuration.Prefix = Prefix;
            configuration.CommandsEnabled = CommandsEnabled;
            configuration.NonCommandChannels = NonCommandChannels.Select(ulong.Parse).ToList();
        }
    }
}
