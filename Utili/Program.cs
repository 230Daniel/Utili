using System;
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
            _config = Config.Load();

            _logger = new Logger
            {
                LogSeverity = LogSeverity.Dbug
            };
            _logger.Initialise();
            _logger.LogEmpty(true);

            if (_config.CacheDatabase)
            {
                _logger.Log("Main", "Cacheing database...", LogSeverity.Info);
                await Database.Database.InitialiseAsync(true, _config.DefaultPrefix);
                _logger.Log("Main", "Database cached", LogSeverity.Info);
            }
            else
            {
                _logger.Log("Main", "Not cacheing database", LogSeverity.Info);
                await Database.Database.InitialiseAsync(false, _config.DefaultPrefix);
            }

            try
            {
                await MainAsync();
            }
            catch(Exception e)
            {
                _logger.ReportError("Main", e);
                Console.WriteLine(e.StackTrace);
            }

            // TODO: Auto-restart or Pterodactyl equivalent
        }

        public static async Task MainAsync()
        {
            _logger.LogEmpty();

            _config = Config.Load();
            _haste = new Haste(_config.HasteServer);

            int[] shardIds = Enumerable.Range(_config.LowerShardId, _config.UpperShardId - (_config.LowerShardId - 1)).ToArray();
            _totalShards = await Database.Sharding.GetTotalShardsAsync();

            _client = new DiscordShardedClient(shardIds, new DiscordSocketConfig 
            {
                TotalShards = _totalShards,
                MessageCacheSize = 10,
                ExclusiveBulkDelete = true,
                LogLevel = Discord.LogSeverity.Info,
                AlwaysDownloadUsers = false,
                
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

            await Task.Delay(-1);
        }
    }
}
