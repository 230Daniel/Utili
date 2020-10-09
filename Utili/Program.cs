using Database.Data;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using System.Diagnostics;
using Discord.Commands;

namespace Utili
{
    class Program
    {
        
        // ReSharper disable InconsistentNaming

        public static Logger _logger;
        public static bool _ready;
        public static Config _config;
        public static DiscordShardedClient _client;

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
            int totalShards = Database.Sharding.GetTotalShards();

            _client = new DiscordShardedClient(shardIds, new DiscordSocketConfig
            {
                ExclusiveBulkDelete = true,
                LogLevel = Discord.LogSeverity.Info,
                TotalShards = totalShards
            });

            _logger.Log("MainAsync", $"Running {_config.UpperShardId - (_config.LowerShardId - 1)} shards of Utili with {totalShards} total shards.", LogSeverity.Info);
            _logger.Log("MainAsync", $"Shard IDs: {_config.LowerShardId} - {_config.UpperShardId}", LogSeverity.Info);
            _logger.LogEmpty();

            _client.Log += Client_Log;
            _client.MessageReceived += Client_MessageReceived;

            await _client.LoginAsync(TokenType.Bot, _config.Token);

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task Client_MessageReceived(SocketMessage partialMessage)
        {
            SocketUserMessage message = partialMessage as SocketUserMessage;
            SocketTextChannel channel = message.Channel as SocketTextChannel;
            SocketGuild guild = channel.Guild;

            SocketCommandContext context = new SocketCommandContext(_client.GetShardFor(guild), message);


        }

        private async Task Client_Log(LogMessage logMessage)
        {
            _logger.Log(logMessage.Source, logMessage.Message, Helper.ConvertToLocalLogSeverity(logMessage.Severity));
        }
    }
}
