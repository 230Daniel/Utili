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
        private readonly DiscordClientBase _client;
        private readonly ILogger<GuildCountService> _logger;
        private readonly IConfiguration _config;

        private Timer _timer;

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

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var shardIds = _config.GetSection("ShardIds").Get<int[]>();
                    var lowerShardId = shardIds.Min();
                    var shardCount = shardIds.Max() - lowerShardId + 1;

                    await Database.Sharding.UpdateShardStatsAsync(shardCount, lowerShardId, _client.GetGuilds().Count);
                    var guilds = await Database.Sharding.GetGuildCountAsync();

                    var tokenConfiguration = _config.GetSection("BotlistTokens").Get<TokenConfiguration>();
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
