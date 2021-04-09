using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Microsoft.Extensions.Configuration;
using Qmmands;
using Utili.Extensions;
using Utili.Utils;

namespace Utili.Commands
{
    public class InfoCommands : DiscordGuildModuleBase
    {
        IConfiguration _config;

        public InfoCommands(IConfiguration config)
        {
            _config = config;
        }

        [Command("About", "Info")]
        public async Task About()
        {
            string domain = _config.GetValue<string>("domain");

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
            string domain = _config.GetValue<string>("domain");
            string dashboardUrl = $"https://{domain}/dashboard/{Context.Guild.Id}";

            LocalEmbedBuilder embed = MessageUtils.CreateEmbed(EmbedType.Info, "Utili",
                $"You can configure Utili on the [dashboard]({dashboardUrl}).\n" +
                $"If you need help, you should [contact us](https://{domain}/contact).\n⠀");

            embed.AddField("**Core**", $"[Command List](https://{domain}/commands)\n" +
                                       $"[Core Settings]({dashboardUrl})");

            embed.AddField("**Channels**", $"[Autopurge]({dashboardUrl}/autopurge)\n" +
                                           $"[Channel Mirroring]({dashboardUrl}/channelmirroring)\n" +
                                           $"[Sticky Notices]({dashboardUrl}/notices)");

            embed.AddField("**Messages**", $"[Message Filter]({dashboardUrl}/messagefilter)\n" +
                                           $"[Message Logging]({dashboardUrl}/messagelogs)\n" +
                                           $"[Message Pinning]({dashboardUrl}/messagepinning)\n" +
                                           $"[Message Voting]({dashboardUrl}/votechannels)");

            embed.AddField("**Users**", $"[Inactive Role]({dashboardUrl}/inactiverole)\n" +
                                        $"[Join Message]({dashboardUrl}/joinmessage)\n" +
                                        $"[Reputation]({dashboardUrl}/reputation)");

            embed.AddField("**Roles**", $"[Join Roles]({dashboardUrl}/joinroles)\n" +
                                        $"[Role Linking]({dashboardUrl}/rolelinking)\n" +
                                        $"[Role Persist]({dashboardUrl}/rolepersist)");

            embed.AddField("**Voice Channels**", $"[Voice Link]({dashboardUrl}/voicelink)\n" +
                                                 $"[Voice Roles]({dashboardUrl}/voiceroles)");

            await Context.Channel.SendEmbedAsync(embed);
        }

        [Command("Ping")]
        public async Task Ping()
        {
            int largestLatency = 0;

            TimeSpan? gatewayLatency = Context.Bot.ApiClient.GatewayApiClient.Heartbeater.Latency;
            int gateway = gatewayLatency.HasValue ? (int)Math.Round(gatewayLatency.Value.TotalMilliseconds) : 0;
            if (gateway > largestLatency) largestLatency = gateway;

            int rest = Program._pingTest.RestLatency;
            if (rest > largestLatency) largestLatency = rest;

            int database = Program._dbPingTest.NetworkLatency;
            if (database > largestLatency) largestLatency = database;
            double databaseQueries = Math.Round(Program._dbPingTest.QueriesPerSecond, 2);

            double cpu = 0;
            double memory = 0;
            if (Program._pingTest.Memory is not null)
            {
                cpu = Program._pingTest.CpuPercentage;
                memory = Math.Round(Program._pingTest.Memory.UsedGigabytes / Program._pingTest.Memory.TotalGigabytes * 100);
            }
            
            PingStatus status = PingStatus.Excellent;
            if (largestLatency > 50) status = PingStatus.Normal;
            if (largestLatency > 250) status = PingStatus.Poor;
            if (largestLatency > 1000) status = PingStatus.Critical;

            if(status < PingStatus.Normal && cpu > 25) status = PingStatus.Normal;
            if(status < PingStatus.Poor && cpu > 50) status = PingStatus.Poor;
            if(status < PingStatus.Critical && cpu > 90 || memory > 95) status = PingStatus.Critical;

            //DateTime upSince = Handlers.ShardHandler.ShardRegister.First(x => x.Item1 == shard.ShardId).Item2;
            //TimeSpan uptime = DateTime.Now - upSince;

#pragma warning disable 8509
            Color color = status switch
            {
                PingStatus.Excellent => new Color(67, 181, 129),
                PingStatus.Normal => new Color(67, 181, 129),
                PingStatus.Poor => new Color(181, 107, 67),
                PingStatus.Critical => new Color(181, 67, 67)
            };
#pragma warning restore 8509

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
