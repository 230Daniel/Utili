using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Microsoft.Extensions.Configuration;
using Qmmands;
using Utili.Extensions;
using Utili.Utils;
using LinuxSystemStats;
using Microsoft.EntityFrameworkCore;
using Database;

namespace Utili.Commands
{
    public class InfoCommands : DiscordGuildModuleBase
    {
        private readonly IConfiguration _config;
        private readonly DatabaseContext _dbContext;

        public InfoCommands(IConfiguration config, DatabaseContext dbContext)
        {
            _config = config;
            _dbContext = dbContext;
        }

        [Command("About", "Info")]
        public async Task About()
        {
            var domain = _config.GetValue<string>("Domain");
            var guilds = await _dbContext.ShardDetails.Where(x => x.Heartbeat > DateTime.UtcNow.AddSeconds(-30)).SumAsync(x => x.Guilds);
            
            var about = string.Concat(
                "Created by 230Daniel#1920\n",
                $"In {guilds} servers\n\n",

                $"[Dashboard](https://{domain}/dashboard)\n",
                "[Discord Server](https://discord.gg/WsxqABZ)\n",
                $"[Contact Us](https://{domain}/contact)\n",
                $"[Get Premium](https://{domain}/premium)\n");

            await Context.Channel.SendInfoAsync("Utili", about);
        }

        [Command("Help", "Commands")]
        public async Task Help()
        {
            var domain = _config.GetValue<string>("Domain");
            var dashboardUrl = $"https://{domain}/dashboard/{Context.Guild.Id}";

            var embed = MessageUtils.CreateEmbed(EmbedType.Info, "Utili",
                    $"You can configure Utili on the [dashboard]({dashboardUrl}).\n" +
                    $"If you need help, you should [contact us](https://{domain}/contact).\n⠀")

                .AddInlineField("**Core**", $"[Command List](https://{domain}/commands)\n" +
                                            $"[Core Settings]({dashboardUrl})")

                .AddInlineField("**Channels**", $"[Autopurge]({dashboardUrl}/autopurge)\n" +
                                                $"[Channel Mirroring]({dashboardUrl}/channelmirroring)\n" +
                                                $"[Sticky Notices]({dashboardUrl}/notices)")

                .AddInlineField("**Messages**", $"[Message Filter]({dashboardUrl}/messagefilter)\n" +
                                                $"[Message Logging]({dashboardUrl}/messagelogs)\n" +
                                                $"[Message Pinning]({dashboardUrl}/messagepinning)\n" +
                                                $"[Message Voting]({dashboardUrl}/votechannels)")

                .AddInlineField("**Users**", $"[Inactive Role]({dashboardUrl}/inactiverole)\n" +
                                             $"[Join Message]({dashboardUrl}/joinmessage)\n" +
                                             $"[Reputation]({dashboardUrl}/reputation)")

                .AddInlineField("**Roles**", $"[Join Roles]({dashboardUrl}/joinroles)\n" +
                                             $"[Role Linking]({dashboardUrl}/rolelinking)\n" +
                                             $"[Role Persist]({dashboardUrl}/rolepersist)")

                .AddInlineField("**Voice Channels**", $"[Voice Link]({dashboardUrl}/voicelink)\n" +
                                                      $"[Voice Roles]({dashboardUrl}/voiceroles)");

            await Context.Channel.SendEmbedAsync(embed);
        }

        [Command("Ping")]
        public async Task Ping()
        {
            var largestLatency = 0;

            TimeSpan? gatewayLatency = DateTime.UtcNow - Context.Message.CreatedAt().UtcDateTime;
            var gateway = (int)Math.Round(gatewayLatency.Value.TotalMilliseconds);
            if (gateway > largestLatency) largestLatency = gateway;

            var sw = Stopwatch.StartNew();
            await Context.Channel.TriggerTypingAsync();
            sw.Stop();
            
            var rest = (int)sw.ElapsedMilliseconds;
            if (rest > largestLatency) largestLatency = rest;

            double cpu = 0;
            double memory = 0;
            try
            {
                cpu = await Stats.GetCurrentCpuUsagePercentageAsync(0);
                var memoryInfo = await Stats.GetMemoryInformationAsync(2);
                memory = Math.Round(memoryInfo.UsedGigabytes / memoryInfo.TotalGigabytes * 100);
            }
            catch { }

            var status = PingStatus.Excellent;
            if (largestLatency > 100) status = PingStatus.Normal;
            if (largestLatency > 500) status = PingStatus.Poor;
            if (largestLatency > 1000) status = PingStatus.Critical;

            if(status < PingStatus.Normal && cpu > 25) status = PingStatus.Normal;
            if(status < PingStatus.Poor && cpu > 50) status = PingStatus.Poor;
            if(status < PingStatus.Critical && cpu > 90 || memory > 95) status = PingStatus.Critical;

            var color = status switch
            {
                PingStatus.Excellent => new Color(67, 181, 129),
                PingStatus.Normal => new Color(67, 181, 129),
                PingStatus.Poor => new Color(181, 107, 67),
                PingStatus.Critical => new Color(181, 67, 67),
                _ => throw new ArgumentOutOfRangeException()
            };

            var embed = MessageUtils.CreateEmbed(EmbedType.Info, $"Pong! Status: {status}");
            embed.WithColor(color);

            embed.AddInlineField("Discord", $"Gateway: {gateway}ms\nRest: {rest}ms");
            embed.AddInlineField("System", $"CPU: {cpu}%\nMemory: {memory}%");
            
            await Context.Channel.SendEmbedAsync(embed);
        }

        private enum PingStatus
        {
            Excellent = 0,
            Normal = 1,
            Poor = 2,
            Critical = 3
        }
    }
}
