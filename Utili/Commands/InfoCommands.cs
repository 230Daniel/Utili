using System;
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
        [Command("Test")] [Permission(Perm.BotOwner)]
        public async Task Test(){
            await Database.Data.Premium.DeleteExpiredSlotsAsync();
        }

        [Command("About"), Alias("Info"), Cooldown(3)]
        public async Task About()
        {
            DiscordSocketClient shard = GetShardForGuild(Context.Guild);

            string about = string.Concat(
                "By 230Daniel#1920\n",
                $"In {await Database.Sharding.GetGuildCountAsync()} servers\n",
                $"Shard {shard.ShardId} ({_totalShards} total)\n",
                $"[Website](https://{_config.Domain})\n",
                $"[Dashboard](https://{_config.Domain}/dashboard)\n",
                $"[Get Premium](https://{_config.Domain}/premium)\n",
                "[Support & Requests Server](https://discord.gg/wGTrDhCaEH)");

            await SendInfoAsync(Context.Channel, "Utili v2 Beta", about);
        }

        [Command("Help"), Alias("Commands")]
        public async Task Help()
        {
            string help = string.Concat(
                $"[List of Commands](https://{_config.Domain}/commands)\n",
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
            if (_pingTest.Memory != null)
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
