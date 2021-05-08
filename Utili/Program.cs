using System;
using System.Linq;
using System.Timers;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Hosting;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;
using Disqord.Gateway.Api;
using Microsoft.Extensions.Configuration;
using Utili.Implementations;
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
                    bot.Token = context.Configuration.GetValue<string>("Token");
                    bot.ReadyEventDelayMode = ReadyEventDelayMode.Guilds;
                    bot.Intents += GatewayIntent.Members;
                    bot.Intents += GatewayIntent.VoiceStates;
                    bot.Activities = new[] { new LocalActivity($"{context.Configuration.GetValue<string>("Domain")} | {context.Configuration.GetValue<string>("DefaultPrefix")}help", ActivityType.Playing)};
                    bot.OwnerIds = new[] { new Snowflake(context.Configuration.GetValue<ulong>("OwnerId")) };

                    int[] shardIds = context.Configuration.GetSection("ShardIds").Get<int[]>();
                    int totalShards = context.Configuration.GetValue<int>("TotalShards");
                    bot.ShardIds = shardIds.Select(x => new ShardId(x, totalShards));
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

            services.AddSingleton(new HasteService(context.Configuration["HasteServer"]));
            services.AddSingleton<RoleCacheService>();
            services.AddSingleton<CommunityService>();
            
            services.AddSingleton<AutopurgeService>();
            services.AddSingleton<ChannelMirroringService>();
            services.AddSingleton<VoiceLinkService>();
            services.AddSingleton<JoinMessageService>();
            services.AddSingleton<MessageFilterService>();
            services.AddSingleton<JoinRolesService>();
            services.AddSingleton<MessageLogsService>();
            services.AddSingleton<ReputationService>();
            services.AddSingleton<RolePersistService>();
            services.AddSingleton<RoleLinkingService>();
            services.AddSingleton<InactiveRoleService>();
            services.AddSingleton<VoiceRolesService>();
            services.AddSingleton<VoteChannelsService>();
            services.AddSingleton<NoticesService>();
        }
    }
}
