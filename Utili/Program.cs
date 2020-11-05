using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using System.Reflection;
using Discord.Commands;
using Utili.Handlers;
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

        public static Features.Autopurge _autopurge = new Features.Autopurge();
        public static Features.VoiceLink _voiceLink = new Features.VoiceLink();
        public static Features.MessageFilter _messageFilter = new Features.MessageFilter();
        public static Features.MessageLogs _messageLogs = new Features.MessageLogs();
        public static Features.VoiceRoles _voiceRoles = new Features.VoiceRoles();

        // ReSharper enable InconsistentNaming

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
                GatewayIntents = GatewayIntents.GuildMembers,
                TotalShards = _totalShards,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 0,

                ExclusiveBulkDelete = true,
                LogLevel = Discord.LogSeverity.Info
            });

            _commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = Discord.LogSeverity.Debug
            });

            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);

            _logger.Log("MainAsync", $"Running {_config.UpperShardId - (_config.LowerShardId - 1)} shards of Utili with {_totalShards} total shards", LogSeverity.Info);
            _logger.Log("MainAsync", $"Shard IDs: {_config.LowerShardId} - {_config.UpperShardId}", LogSeverity.Info);
            _logger.LogEmpty();

            _client.Log += Client_Log;
            _client.MessageReceived += MessagesHandler.MessageReceived;
            _client.MessageUpdated += MessagesHandler.MessageEdited;
            _client.MessageDeleted += MessagesHandler.MessageDeleted;
            _client.MessagesBulkDeleted += MessagesHandler.MessagesBulkDeleted;
            _client.ShardReady += ReadyHandler.ShardReady;
            _client.UserVoiceStateUpdated += VoiceHandler.UserVoiceStateUpdated;

            await _client.LoginAsync(TokenType.Bot, _config.Token);

            await _client.StartAsync();

            _autopurge.Start();
            _voiceLink.Start();
            _voiceRoles.Start();

            await Task.Delay(-1);
        }

        private async Task Client_Log(LogMessage logMessage)
        {
            _logger.Log(logMessage.Source, logMessage.Message, Helper.ConvertToLocalLogSeverity(logMessage.Severity));
        }
    }
}
