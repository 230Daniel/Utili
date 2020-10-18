using Database.Data;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using System.Diagnostics;
using System.Reflection;
using Discord.Commands;
using Utili.Handlers;
using Discord.Rest;
using System.Timers;

namespace Utili
{
    internal class Program
    {
        
        // ReSharper disable InconsistentNaming

        public static DiscordShardedClient _client;
        public static CommandService _commands;

        public static Logger _logger;
        public static Config _config;
        public static bool _ready;
        public static int _totalShards;

        public static Timer _shardStatsUpdater;

        // ReSharper enable InconsistentNaming

        private static Features.Autopurge _autopurge = new Features.Autopurge();

        private static void Main()
        {
            _logger = new Logger
            {
                LogSeverity = LogSeverity.Dbug
            };
            _logger.Initialise();
            _logger.LogEmpty(true);

            _logger.Log("Main", "Connecting to the database", LogSeverity.Info);

            // Initialise the database and use cache
            Database.Database.Initialise(true);

            _logger.Log("Main", "Connected to the database", LogSeverity.Info);
            _logger.Log("Main", "Cache downloaded", LogSeverity.Info);

            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            _logger.LogEmpty();

            _ready = false;

            _config = Config.Load();

            int[] shardIds = Enumerable.Range(_config.LowerShardId, _config.UpperShardId - (_config.LowerShardId - 1)).ToArray();
            _totalShards = Database.Sharding.GetTotalShards();

            _client = new DiscordShardedClient(shardIds, new DiscordSocketConfig
            {
                ExclusiveBulkDelete = true,
                LogLevel = Discord.LogSeverity.Info,
                TotalShards = _totalShards
            });

            _commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = Discord.LogSeverity.Debug
            });

            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);

            _logger.Log("MainAsync", $"Running {_config.UpperShardId - (_config.LowerShardId - 1)} shards of Utili with {_totalShards} total shards.", LogSeverity.Info);
            _logger.Log("MainAsync", $"Shard IDs: {_config.LowerShardId} - {_config.UpperShardId}", LogSeverity.Info);
            _logger.LogEmpty();

            _client.Log += Client_Log;
            _client.MessageReceived += MessageReceivedHandler.MessageReceived;
            _client.ShardReady += ReadyHandler.ShardReady;

            await _client.LoginAsync(TokenType.Bot, _config.Token);

            await _client.StartAsync();

            _autopurge.Start();

            await Task.Delay(-1);
        }

        private async Task Client_Log(LogMessage logMessage)
        {
            _logger.Log(logMessage.Source, logMessage.Message, Helper.ConvertToLocalLogSeverity(logMessage.Severity));
        }
    }
}
