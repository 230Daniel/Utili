using System.Threading.Tasks;
using Database;
using Database.Entities;
using Database.Extensions;

namespace Utili.Extensions
{
    public static class DatabaseContextExtensions
    {
        public static string DefaultPrefix { get; internal set; }

        public static async Task SetHasFeatureAsync(this DatabaseContext dbContext, ulong guildId, BotFeatures feature, bool enabled)
        {
            var coreConfig = await dbContext.CoreConfigurations.GetForGuildAsync(guildId);

            if (coreConfig is null)
            {
                coreConfig = new CoreConfiguration(guildId)
                {
                    Prefix = DefaultPrefix,
                    CommandsEnabled = true,
                    NonCommandChannels = new()
                };
                coreConfig.SetHasFeature(feature, enabled);
                dbContext.CoreConfigurations.Add(coreConfig);
            }
            else
            {
                coreConfig.SetHasFeature(feature, enabled);
                dbContext.CoreConfigurations.Update(coreConfig);
            }
        }
    }
}
