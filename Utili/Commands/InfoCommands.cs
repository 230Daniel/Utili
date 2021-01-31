using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using static Utili.Program;
using static Utili.MessageSender;
using static Utili.Helper;

namespace Utili.Commands
{
    public class InfoCommands : ModuleBase<SocketCommandContext>
    {
        [Command("About"), Alias("Info")]
        public async Task About()
        {
            string about = string.Concat(
                "Created by 230Daniel#1920\n",
                $"In {await Database.Sharding.GetGuildCountAsync()} servers\n\n",

                $"[Dashboard](https://{_config.Domain}/dashboard)\n",
                "[Discord Server](https://discord.gg/WsxqABZ)\n",
                $"[Contact Us](https://{_config.Domain}/contact)\n",
                $"[Get Premium](https://{_config.Domain}/premium)\n");

            await SendInfoAsync(Context.Channel, "Utili", about);
        }

        [Command("Help"), Alias("Commands")]
        public async Task Help()
        {
            string help = string.Concat(
                $"[Commands](https://{_config.Domain}/commands)\n",
                $"[Dashboard](https://{_config.Domain}/dashboard/{Context.Guild.Id}/core)\n");

            await SendInfoAsync(Context.Channel, "Utili", help);
        }

        [Command("Ping"), Alias("Lag")]
        public async Task Ping()
        {
            int largestLatency = 0;
            DiscordSocketClient shard = GetShardForGuild(Context.Guild);

            int gateway = shard.Latency;
            if (gateway > largestLatency) largestLatency = gateway;

            int rest = _pingTest.RestLatency;
            if (rest > largestLatency) largestLatency = rest;

            int database = _dbPingTest.NetworkLatency;
            if (database > largestLatency) largestLatency = database;
            double databaseQueries = Math.Round(_dbPingTest.QueriesPerSecond, 2);

            double cpu = 0;
            double memory = 0;
            if (_pingTest.Memory is not null)
            {
                cpu = _pingTest.CpuPercentage;
                memory = Math.Round(_pingTest.Memory.UsedGigabytes / _pingTest.Memory.TotalGigabytes * 100);
            }
            
            PingStatus status = PingStatus.Excellent;
            if (largestLatency > 50) status = PingStatus.Normal;
            if (largestLatency > 250) status = PingStatus.Poor;
            if (largestLatency > 1000) status = PingStatus.Critical;

            if(status < PingStatus.Normal && cpu > 25) status = PingStatus.Normal;
            if(status < PingStatus.Poor && cpu > 50) status = PingStatus.Poor;
            if(status < PingStatus.Critical && cpu > 90 || memory > 95) status = PingStatus.Critical;

            DateTime upSince = Handlers.ShardHandler.ShardRegister.First(x => x.Item1 == shard.ShardId).Item2;
            TimeSpan uptime = DateTime.Now - upSince;

#pragma warning disable 8509
            Color color = status switch
            {
                PingStatus.Excellent => new Color(67, 181, 129),
                PingStatus.Normal => new Color(67, 181, 129),
                PingStatus.Poor => new Color(181, 107, 67),
                PingStatus.Critical => new Color(181, 67, 67)
            };
#pragma warning restore 8509

            EmbedBuilder embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder { Name = $"Pong! Status: {status}" },
                Color = color
            };

            embed.AddField("Discord", $"Api: {gateway}ms\nRest: {rest}ms", true);
            embed.AddField("Database", $"Latency: {database}ms\nQueries: {databaseQueries}/s", true);
            embed.AddField("System", $"CPU: {cpu}%\nMem: {memory}%", true);

            embed.WithFooter($"Shard {shard.ShardId} uptime: {uptime.ToShortString()}");
            await SendEmbedAsync(Context.Channel, embed.Build());
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
