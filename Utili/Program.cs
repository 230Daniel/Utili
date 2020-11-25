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
    internal class Program
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
        public static PingTest _pingTest = new PingTest();
        public static Database.PingTest _dbPingTest = new Database.PingTest();

        public static Autopurge _autopurge = new Autopurge();
        public static ChannelMirroring _channelMirroring = new ChannelMirroring();
        public static InactiveRole _inactiveRole = new InactiveRole();
        public static JoinMessage _joinMessage = new JoinMessage();
        public static Notices _notices = new Notices();
        public static Reputation _reputation = new Reputation();
        public static Roles _roles = new Roles();
        public static VoiceLink _voiceLink = new VoiceLink();
        public static MessageFilter _messageFilter = new MessageFilter();
        public static MessageLogs _messageLogs = new MessageLogs();
        public static VoiceRoles _voiceRoles = new VoiceRoles();
        public static VoteChannels _voteChannels = new VoteChannels();

        public static RoslynEngine _roslyn = new RoslynEngine();

        // ReSharper enable InconsistentNaming

        private static async Task Main()
        {
            _logger = new Logger
            {
                LogSeverity = LogSeverity.Dbug
            };
            _logger.Initialise();
            _logger.LogEmpty(true);

            _logger.Log("Main", "Downloading database cache", LogSeverity.Info);
            Database.Database.Initialise(true);
            _logger.Log("Main", "Database cache downloaded", LogSeverity.Info);

            await new Program().MainAsync();

            // TODO: Auto-restart or Pterodactyl equivalent
        }

        public async Task MainAsync()
        {
            _logger.LogEmpty();

            _config = Config.Load();
            _haste = new Haste(_config.HasteServer);

            int[] shardIds = Enumerable.Range(_config.LowerShardId, _config.UpperShardId - (_config.LowerShardId - 1)).ToArray();
            _totalShards = Database.Sharding.GetTotalShards();

            _client = new DiscordShardedClient(shardIds, new DiscordSocketConfig 
            {
                TotalShards = _totalShards,
                MessageCacheSize = 5,
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

            _logger.Log("MainAsync", $"Running {_config.UpperShardId - (_config.LowerShardId - 1)} shards of Utili with {_totalShards} total shards", LogSeverity.Info);
            _logger.Log("MainAsync", $"Shard IDs: {_config.LowerShardId} - {_config.UpperShardId}", LogSeverity.Info);
            _logger.LogEmpty();

            _client.Log += ShardHandler.Log;
            _client.ShardConnected += ShardHandler.ShardConnected;

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

            _autopurge.Start();
            _voiceLink.Start();
            _voiceRoles.Start();
            _inactiveRole.Start();
            _notices.Start();

            await Task.Delay(-1);
        }
    }
}
