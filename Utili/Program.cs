using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Hosting;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;
using Disqord.Gateway.Api;
using Disqord.Gateway.Default;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Utili.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Database;
using Serilog;
using Utili.Extensions;
using Utili.Features;
using Utili.Services;

namespace Utili
{
    internal static class Program
    {
        private static async Task Main()
        {
            var host = Host.CreateDefaultBuilder()
                .UseSystemd()
                .UseSerilog()
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

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(host.Services.GetRequiredService<IConfiguration>())
                .CreateLogger();
            
            try
            {
                Log.Information("Migrating database");
                using (var scope = host.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    await db.Database.MigrateAsync();
                }

                Log.Information("Running host");
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Crashed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(Logger<>));

            services.AddDbContext<DatabaseContext>();
            services.AddScoped<CoreConfigurationCacheService>();

            services.AddInteractivityExtension();
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
            DatabaseContextExtensions.DefaultPrefix = context.Configuration["DefaultPrefix"];
        }
    }
}
