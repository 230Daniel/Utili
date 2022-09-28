using System;
using System.Threading.Tasks;
using System.Timers;
using BotlistStatsPoster;
using Disqord.Gateway;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Utili.Bot.Services;

public class GuildCountService
{
    private readonly UtiliDiscordBot _bot;
    private readonly ILogger<GuildCountService> _logger;
    private readonly IConfiguration _config;

    private Timer _timer;

    public GuildCountService(ILogger<GuildCountService> logger, UtiliDiscordBot bot, IConfiguration config)
    {
        _logger = logger;
        _bot = bot;
        _config = config;

        _timer = new Timer(300000);
        _timer.Elapsed += Timer_Elapsed;
    }

    public void Start()
    {
        _timer.Start();
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                if (!_config.GetValue<bool>("BotList:Enable")) return;

                var guildCount = _bot.GetGuilds().Count;

                var tokenConfiguration = _config.GetSection("BotList:Tokens").Get<TokenConfiguration>();
                StatsPoster poster = new(_bot.CurrentUser.Id, tokenConfiguration);
                await poster.PostGuildCountAsync(guildCount);

                _logger.LogDebug("Successfully posted {Guilds} guilds to the botlists", guildCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on timer elapsed");
            }
        });
    }
}
