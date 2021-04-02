using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using Database;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Utili.Features;
using Utili.Handlers;

namespace Utili
{
    internal static class Program
    {
        
        // ReSharper disable InconsistentNaming

        public static DiscordShardedClient _client;
        public static CommandService _commands;
        public static DiscordRestClient _rest => _client.Rest;

        public static Logger _logger;
        public static Config _config;
        public static Haste _haste;
        public static int _totalShards;

        public static Timer _shardStatsUpdater;
        public static readonly PingTest _pingTest = new PingTest();
        public static readonly Database.PingTest _dbPingTest = new Database.PingTest();

        // ReSharper enable InconsistentNaming

        private static async Task Main()
        {
            List<DateTime> crashes = new List<DateTime>();
            while (true)
            {
                _config = Config.Load();
                _logger = new Logger(LogSeverity.Dbug);
                _logger.LogEmpty(true);

                Console.ForegroundColor = ConsoleColor.Blue;
                if (crashes.Count(x => x > DateTime.Now - TimeSpan.FromMinutes(5)) >= 3)
                {
                    _logger.Log("Main", "Three crashes have occurred in the last 5 minutes.", LogSeverity.Warn);
                    _logger.Log("Main", "Waiting for 10 minutes", LogSeverity.Warn);
                    crashes.Clear();
                    await Task.Delay(60000 * 10);
                }

                if (_config.CacheDatabase)
                {
                    _logger.Log("Main", "Caching database...", LogSeverity.Info);
                    await Database.Database.InitialiseAsync(true, _config.DefaultPrefix);
                    _logger.Log("Main", "Database cached", LogSeverity.Info);
                }
                else
                {
                    _logger.Log("Main", "Database caching is disabled", LogSeverity.Info);
                    await Database.Database.InitialiseAsync(false, _config.DefaultPrefix);
                }

                try
                {
                    await MainAsync();
                }
                catch(Exception e)
                {
                    _logger.ReportError("Main", e, LogSeverity.Crit);
                    crashes.Add(DateTime.Now);
                    await Task.Delay(5000);
                }
            }
        }

        private static async Task MainAsync()
        {
            _logger.LogEmpty();
            _config = Config.Load();
            _haste = new Haste(_config.HasteServer);

            int[] shardIds = Enumerable.Range(_config.LowerShardId, _config.UpperShardId - (_config.LowerShardId - 1)).ToArray();
            _totalShards = await Database.Sharding.GetTotalShardsAsync();

            _client = new DiscordShardedClient(shardIds, new DiscordSocketConfig 
            {
                TotalShards = _totalShards,
                MessageCacheSize = 0,
                ExclusiveBulkDelete = true,
                LogLevel = Discord.LogSeverity.Info,
                AlwaysDownloadUsers = false,
                DefaultRetryMode = RetryMode.AlwaysFail,
                GatewayIntents = 
                    GatewayIntents.Guilds | 
                    GatewayIntents.GuildMembers | 
                    GatewayIntents.GuildMessageReactions | 
                    GatewayIntents.GuildMessages | 
                    GatewayIntents.GuildVoiceStates
            });

            _commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = Discord.LogSeverity.Info
            });
            _commands.AddTypeReader(typeof(IUser), new UserTypeReader());
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            if(!_config.Production) _logger.Log("MainAsync", "Utili is not in production mode", LogSeverity.Warn);
            _logger.Log("MainAsync", $"Running {_config.UpperShardId - (_config.LowerShardId - 1)} shards of Utili with {_totalShards} total shards", LogSeverity.Info);
            _logger.Log("MainAsync", $"Shard IDs: {_config.LowerShardId} - {_config.UpperShardId}", LogSeverity.Info);
            _logger.LogEmpty();

            _client.Log += ShardHandler.Log;
            _client.ShardReady += ShardHandler.ShardReady;
            _client.JoinedGuild += ShardHandler.JoinedGuild;
            _client.LeftGuild += ShardHandler.LeftGuild;

            _client.MessageReceived += MessageHandler.MessageReceived;
            _client.MessageUpdated += MessageHandler.MessageEdited;
            _client.MessageDeleted += MessageHandler.MessageDeleted;
            _client.MessagesBulkDeleted += MessageHandler.MessagesBulkDeleted;

            _client.ReactionAdded += ReactionHandler.ReactionAdded;
            _client.ReactionRemoved += ReactionHandler.ReactionRemoved;
            _client.ReactionsCleared += ReactionHandler.ReactionsCleared;
            _client.ReactionsRemovedForEmote += ReactionHandler.ReactionsRemovedForEmote;

            _client.UserVoiceStateUpdated += VoiceHandler.UserVoiceStateUpdated;
            _client.UserJoined += GuildHandler.UserJoined;
            _client.GuildMemberUpdated += GuildHandler.UserUpdated;
            _client.UserLeft += GuildHandler.UserLeft;

            await _client.SetGameAsync("Starting up...");
            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.StartAsync();

            _pingTest.Start();
            _dbPingTest.Start();

            Autopurge.Start();
            VoiceLink.Start();
            VoiceRoles.Start();
            InactiveRole.Start();
            Notices.Start();
            JoinRoles.Start();

            await Task.Delay(-1);
        }
    }
}
