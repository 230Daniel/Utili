using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Hosting;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;
using Disqord.Gateway.Default;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Utili.Database;
using Serilog;
using Utili.Bot.Extensions;
using Utili.Bot.Features;
using Utili.Bot.Services;

namespace Utili.Bot;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .UseSystemd()
            .UseSerilog()
            .ConfigureServices(ConfigureServices)
            .ConfigureDiscordBot<UtiliDiscordBot>((context, bot) =>
            {
                bot.Token = context.Configuration.GetValue<string>("Discord:Token");
                bot.ReadyEventDelayMode = ReadyEventDelayMode.Guilds;
                bot.Intents |= GatewayIntents.Members;
                bot.Intents |= GatewayIntents.VoiceStates;
                bot.Activities = new[]
                {
                    new LocalActivity($"{context.Configuration.GetValue<string>("Services:WebsiteDomain")} | Starting up...", ActivityType.Playing)
                };
                bot.OwnerIds = new[]
                {
                    new Snowflake(context.Configuration.GetValue<ulong>("Discord:OwnerId"))
                };
            })
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(host.Services.GetRequiredService<IConfiguration>())
            .CreateLogger();

        try
        {
            if (args.Contains("--no-database-migration"))
                Log.Information("Skipping database migration");
            else
            {
                Log.Information("Migrating database");
                using var scope = host.Services.CreateScope();
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
        services.AddSingleton<CommunityService>();
        services.AddSingleton<GuildCountService>();
        services.AddSingleton<WebhookService>();
        services.AddSingleton<IsPremiumService>();

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
        DatabaseContextExtensions.DefaultPrefix = context.Configuration["Discord:DefaultPrefix"];
    }
}
