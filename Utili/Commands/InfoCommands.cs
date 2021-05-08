using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway.Api;
using Disqord.Rest;
using Microsoft.Extensions.Configuration;
using Qmmands;
using Utili.Extensions;
using Utili.Utils;
using LinuxSystemStats;

namespace Utili.Commands
{
    public class InfoCommands : DiscordGuildModuleBase
    {
        IConfiguration _config;
        IGatewayHeartbeater _heartbeater;

        public InfoCommands(IConfiguration config, IGatewayHeartbeater heartbeater)
        {
            _config = config;
            _heartbeater = heartbeater;
        }

        [Command("About", "Info")]
        public async Task About()
        {
            string domain = _config.GetValue<string>("Domain");

            string about = string.Concat(
                "Created by 230Daniel#1920\n",
                $"In {await Database.Sharding.GetGuildCountAsync()} servers\n\n",

                $"[Dashboard](https://{domain}/dashboard)\n",
                "[Discord Server](https://discord.gg/WsxqABZ)\n",
                $"[Contact Us](https://{domain}/contact)\n",
                $"[Get Premium](https://{domain}/premium)\n");

            await Context.Channel.SendInfoAsync("Utili", about);
        }

        [Command("Help", "Commands")]
        public async Task Help()
        {
            string domain = _config.GetValue<string>("Domain");
            string dashboardUrl = $"https://{domain}/dashboard/{Context.Guild.Id}";

            LocalEmbedBuilder embed = MessageUtils.CreateEmbed(EmbedType.Info, "Utili",
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
            int largestLatency = 0;

            TimeSpan? gatewayLatency = DateTime.UtcNow - Context.Message.CreatedAt.UtcDateTime;
            int gateway = (int)Math.Round(gatewayLatency.Value.TotalMilliseconds);
            if (gateway > largestLatency) largestLatency = gateway;

            Stopwatch sw = Stopwatch.StartNew();
            await Context.Channel.TriggerTypingAsync();
            sw.Stop();
            
            int rest = (int)sw.ElapsedMilliseconds;
            if (rest > largestLatency) largestLatency = rest;

            int database = Database.Status.Latency;
            if (database > largestLatency) largestLatency = database;
            double databaseQueries = Math.Round(Database.Status.QueriesPerSecond, 2);

            double cpu = 0;
            double memory = 0;
            try
            {
                cpu = await Stats.GetCurrentCpuUsagePercentageAsync(0);
                MemoryInformation memoryInfo = await Stats.GetMemoryInformationAsync(2);
                memory = Math.Round(memoryInfo.UsedGigabytes / memoryInfo.TotalGigabytes * 100);
            }
            catch { }

            PingStatus status = PingStatus.Excellent;
            if (largestLatency > 100) status = PingStatus.Normal;
            if (largestLatency > 500) status = PingStatus.Poor;
            if (largestLatency > 1000) status = PingStatus.Critical;

            if(status < PingStatus.Normal && cpu > 25) status = PingStatus.Normal;
            if(status < PingStatus.Poor && cpu > 50) status = PingStatus.Poor;
            if(status < PingStatus.Critical && cpu > 90 || memory > 95) status = PingStatus.Critical;

            //DateTime upSince = Handlers.ShardHandler.ShardRegister.First(x => x.Item1 == shard.ShardId).Item2;
            //TimeSpan uptime = DateTime.Now - upSince;
            
            Color color = status switch
            {
                PingStatus.Excellent => new Color(67, 181, 129),
                PingStatus.Normal => new Color(67, 181, 129),
                PingStatus.Poor => new Color(181, 107, 67),
                PingStatus.Critical => new Color(181, 67, 67),
                _ => throw new ArgumentOutOfRangeException()
            };

            LocalEmbedBuilder embed = MessageUtils.CreateEmbed(EmbedType.Info, $"Pong! Status: {status}");
            embed.WithColor(color);

            embed.AddField("Discord", $"Api: {gateway}ms\nRest: {rest}ms", true);
            embed.AddField("Database", $"Latency: {database}ms\nQueries: {databaseQueries}/s", true);
            embed.AddField("System", $"CPU: {cpu}%\nMem: {memory}%", true);

            //embed.WithFooter($"Shard {shard.ShardId} uptime: {uptime.ToShortString()}");
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
