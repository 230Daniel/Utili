using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using BotlistStatsPoster;
using Disqord;
using Disqord.Gateway;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Utili.Services
{
    public class GuildCountService
    {
        DiscordClientBase _client;
        ILogger<GuildCountService> _logger;
        IConfiguration _config;
        
        Timer _timer;

        public GuildCountService(ILogger<GuildCountService> logger, DiscordClientBase client, IConfiguration config)
        {
            _logger = logger;
            _client = client;
            _config = config;
        }

        public void Start()
        {
            if (!_config.GetValue<bool>("PostToBotlist")) return;
            _timer = new Timer(10000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    int[] shardIds = _config.GetSection("ShardIds").Get<int[]>();
                    int lowerShardId = shardIds.Min();
                    int shardCount = shardIds.Max() - lowerShardId + 1;

                    await Database.Sharding.UpdateShardStatsAsync(shardCount, lowerShardId, _client.GetGuilds().Count);
                    int guilds = await Database.Sharding.GetGuildCountAsync();

                    TokenConfiguration tokenConfiguration = _config.GetSection("BotlistTokens").Get<TokenConfiguration>();
                    StatsPoster poster = new(_client.CurrentUser.Id, tokenConfiguration);
                    await poster.PostGuildCountAsync(guilds);
                    _logger.LogDebug($"Successfully posted {guilds} guilds to the botlists");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception thrown on timer elapsed");
                }
            });
        }
    }
}