using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using BotlistStatsPoster;
using Disqord.Gateway;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Utili.Database.Entities;
using Utili.Database.Extensions;
using Utili.Bot.Extensions;

namespace Utili.Bot.Services;

public class GuildCountService
{
    private readonly UtiliDiscordBot _client;
    private readonly ILogger<GuildCountService> _logger;
    private readonly IConfiguration _config;
    private readonly IServiceScopeFactory _scopeFactory;

    private Timer _timer;
    private int _counter;

    public GuildCountService(ILogger<GuildCountService> logger, UtiliDiscordBot client, IConfiguration config, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _client = client;
        _config = config;
        _scopeFactory = scopeFactory;

        _timer = new Timer(10000);
        _timer.Elapsed += Timer_Elapsed;
    }

    public void Start()
    {
        _timer.Start();
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var shardIds = _config.GetSection("Discord:ShardIds").Get<int[]>();
                var totalShards = (ulong) _config.GetValue<int>("Discord:TotalShards");

                using var scope = _scopeFactory.CreateScope();
                var db = scope.GetDbContext();
                var records = await db.ShardDetails.Where(x => shardIds.Contains(x.ShardId)).ToListAsync();

                var now = DateTime.UtcNow;

                foreach (var shardId in shardIds.Where(x => records.All(y => y.ShardId != x)))
                {
                    var record = new ShardDetail(shardId)
                    {
                        Guilds = GetShardGuildCount((ulong)shardId, totalShards),
                        Heartbeat = now
                    };
                    db.ShardDetails.Add(record);
                }

                foreach (var record in records)
                {
                    record.Guilds = GetShardGuildCount((ulong)record.ShardId, totalShards);
                    record.Heartbeat = now;
                    db.ShardDetails.Update(record);
                }

                await db.SaveChangesAsync();

                _counter++;
                if (_counter <= 30) return;

                _counter = 0;
                if (!_config.GetValue<bool>("BotList:Enable")) return;

                var guilds = await db.ShardDetails.GetTotalGuildCountAsync();

                var tokenConfiguration = _config.GetSection("BotList:Tokens").Get<TokenConfiguration>();
                StatsPoster poster = new(_client.CurrentUser.Id, tokenConfiguration);
                await poster.PostGuildCountAsync(guilds);

                _logger.LogDebug("Successfully posted {Guilds} guilds to the botlists", guilds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on timer elapsed");
            }
        });
    }

    private int GetShardGuildCount(ulong shardId, ulong totalShards)
    {
        return _client.GetGuilds().Count(x => (x.Key >> 22) % totalShards == shardId);
    }
}
