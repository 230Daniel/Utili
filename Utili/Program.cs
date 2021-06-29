using System;
using System.Linq;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Hosting;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;
using Disqord.Gateway.Api;
using Disqord.Gateway.Default;
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
        private static void Main()
        {
            var host = Host.CreateDefaultBuilder()
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
                    bot.Activities = new[] { new LocalActivity($"{context.Configuration.GetValue<string>("Domain")} | Starting up...", ActivityType.Playing)};
                    bot.OwnerIds = new[] { new Snowflake(context.Configuration.GetValue<ulong>("OwnerId")) };

                    var shardIds = context.Configuration.GetSection("ShardIds").Get<int[]>();
                    var totalShards = context.Configuration.GetValue<int>("TotalShards");
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

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddInteractivity();
            services.AddPrefixProvider<PrefixProvider>();

            services.AddSingleton<MemberCacheService>();
            services.AddSingleton<HasteService>();
            services.AddSingleton<CommunityService>();
            services.AddSingleton<GuildCountService>();
            
            services.AddSingleton<AutopurgeService>();
            services.AddSingleton<ChannelMirroringService>();
            services.AddSingleton<InactiveRoleService>();
            services.AddSingleton<JoinMessageService>();
            services.AddSingleton<JoinRolesService>();
            services.AddSingleton<MessageFilterService>();
            services.AddSingleton<MessageLogsService>();
            services.AddSingleton<NoticesService>();
            services.AddSingleton<ReputationService>();
            services.AddSingleton<RoleLinkingService>();
            services.AddSingleton<RolePersistService>();
            services.AddSingleton<VoiceLinkService>();
            services.AddSingleton<VoiceRolesService>();
            services.AddSingleton<VoteChannelsService>();
            
            services.Configure<DefaultGatewayCacheProviderConfiguration>(x => x.MessagesPerChannel = 1);
        }
    }
}
