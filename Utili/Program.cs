using System;
using System.Timers;
using Database;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Hosting;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;
using DisqordTestBot.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Utili.Features;
using Utili.Services;

namespace Utili
{
    internal static class Program
    {
        
        // ReSharper disable InconsistentNaming

        public static Discord.WebSocket.DiscordShardedClient _oldClient;
        public static Discord.Commands.CommandService _oldCommands;
        public static Discord.Rest.DiscordRestClient _oldRest => _oldClient.Rest;

        public static Logger _logger;
        public static Config _config;
        public static Haste _haste;
        public static int _totalShards;

        public static Timer _shardStatsUpdater;
        public static readonly PingTest _pingTest = new PingTest();
        public static readonly Database.PingTest _dbPingTest = new Database.PingTest();

        // ReSharper enable InconsistentNaming

        static void Main()
        {
            IHost host = Host.CreateDefaultBuilder()
                .ConfigureLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddProvider(new LoggerProvider());
                })
                .ConfigureServices(ConfigureServices)
                .ConfigureDiscordBotSharder<MyDiscordBotSharder>((context, bot) =>
                {
                    bot.Token = context.Configuration["token"];
                    bot.UseMentionPrefix = true;
                    bot.ReadyEventDelayMode = ReadyEventDelayMode.Guilds;
                    bot.Intents += GatewayIntent.Members;
                    bot.Intents += GatewayIntent.VoiceStates;
                    bot.Activities = new[] { new LocalActivity("v3 ?!??", ActivityType.Playing)};
                    bot.OwnerIds = new[] { new Snowflake(ulong.Parse(context.Configuration["ownerId"])) };
                })
                .Build();

            try
            {
                using (host)
                {
                    host.Run();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
        }

        static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddInteractivity();
            services.AddPrefixProvider<PrefixProvider>();
            services.AddSingleton<AutopurgeService>();
        }

        //private static async Task MainAsync()
        //{
        //    _logger.LogEmpty();
        //    _config = Config.Load();
        //    _haste = new Haste(_config.HasteServer);

        //    int[] shardIds = Enumerable.Range(_config.LowerShardId, _config.UpperShardId - (_config.LowerShardId - 1)).ToArray();
        //    _totalShards = await Database.Sharding.GetTotalShardsAsync();

        //    _client = new DiscordShardedClient(shardIds, new DiscordSocketConfig 
        //    {
        //        TotalShards = _totalShards,
        //        MessageCacheSize = 0,
        //        ExclusiveBulkDelete = true,
        //        LogLevel = Discord.LogSeverity.Info,
        //        AlwaysDownloadUsers = false,
        //        DefaultRetryMode = RetryMode.AlwaysFail,
        //        GatewayIntents = 
        //            GatewayIntents.Guilds | 
        //            GatewayIntents.GuildMembers | 
        //            GatewayIntents.GuildMessageReactions | 
        //            GatewayIntents.GuildMessages | 
        //            GatewayIntents.GuildVoiceStates
        //    });

        //    _commands = new CommandService(new CommandServiceConfig
        //    {
        //        CaseSensitiveCommands = false,
        //        DefaultRunMode = RunMode.Async,
        //        LogLevel = Discord.LogSeverity.Info
        //    });
        //    _commands.AddTypeReader(typeof(IUser), new UserTypeReader());
        //    await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);

        //    if(!_config.Production) _logger.Log("MainAsync", "Utili is not in production mode", LogSeverity.Warn);
        //    _logger.Log("MainAsync", $"Running {_config.UpperShardId - (_config.LowerShardId - 1)} shards of Utili with {_totalShards} total shards", LogSeverity.Info);
        //    _logger.Log("MainAsync", $"Shard IDs: {_config.LowerShardId} - {_config.UpperShardId}", LogSeverity.Info);
        //    _logger.LogEmpty();

        //    _client.Log += ShardHandler.Log;
        //    _client.ShardReady += ShardHandler.ShardReady;
        //    _client.JoinedGuild += ShardHandler.JoinedGuild;
        //    _client.LeftGuild += ShardHandler.LeftGuild;

        //    _client.MessageReceived += MessageHandler.MessageReceived;
        //    _client.MessageUpdated += MessageHandler.MessageEdited;
        //    _client.MessageDeleted += MessageHandler.MessageDeleted;
        //    _client.MessagesBulkDeleted += MessageHandler.MessagesBulkDeleted;

        //    _client.ReactionAdded += ReactionHandler.ReactionAdded;
        //    _client.ReactionRemoved += ReactionHandler.ReactionRemoved;
        //    _client.ReactionsCleared += ReactionHandler.ReactionsCleared;
        //    _client.ReactionsRemovedForEmote += ReactionHandler.ReactionsRemovedForEmote;

        //    _client.UserVoiceStateUpdated += VoiceHandler.UserVoiceStateUpdated;
        //    _client.UserJoined += GuildHandler.UserJoined;
        //    _client.GuildMemberUpdated += GuildHandler.UserUpdated;
        //    _client.UserLeft += GuildHandler.UserLeft;

        //    await _client.SetGameAsync("Starting up...");
        //    await _client.LoginAsync(TokenType.Bot, _config.Token);
        //    await _client.StartAsync();

        //    _pingTest.Start();
        //    _dbPingTest.Start();

        //    VoiceLink.Start();
        //    VoiceRoles.Start();
        //    InactiveRole.Start();
        //    Notices.Start();
        //    JoinRoles.Start();

        //    await Task.Delay(-1);
        //}
    }
}
