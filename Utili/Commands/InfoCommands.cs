using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using static Utili.Program;
using static Utili.MessageSender;
using static Utili.Helper;
using System.Linq;

namespace Utili.Commands
{
    public class InfoCommands : ModuleBase<SocketCommandContext>
    {
        [Command("About"), Alias("Info"), Cooldown(3)]
        public async Task About()
        {
            DiscordSocketClient shard = GetShardForGuild(Context.Guild);

            string about = string.Concat(
                "By 230Daniel#1920",
                $"In {Database.Sharding.GetGuildCount()} servers",
                $"Shard {shard.ShardId} ({_totalShards} total)",
                $"[Website](https://{_config.Domain})",
                $"[Dashboard](https://{_config.Domain}/dashboard)",
                $"[Get Premium](https://{_config.Domain}/premium)",
                "[Support & Requests Server](https://discord.gg/hCYWk9x)");

            await SendInfoAsync(Context.Channel, "Utili v2 Beta", about);
        }

        [Command("Help"), Alias("Commands")]
        public async Task Help()
        {
            string help = string.Concat(
                $"[List of Commands](https://{_config.Domain}/commands)",
                "[Dashboard](https://{_config.Domain}/dashboard/{Context.Guild.Id}/core)");

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

            PingStatus status = PingStatus.Excellent;
            if (largestLatency > 50) status = PingStatus.Normal;
            if (largestLatency > 250) status = PingStatus.Poor;
            if (largestLatency > 1000) status = PingStatus.Critical;

            Color color = status switch
            {
                PingStatus.Excellent => new Color(67, 181, 129),
                PingStatus.Normal => new Color(67, 181, 129),
                PingStatus.Poor => new Color(181, 107, 67),
                PingStatus.Critical => new Color(181, 67, 67)
            };

            EmbedBuilder embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder { Name = $"Pong! Status: {status}" },
                Color = color,
                Footer = new EmbedFooterBuilder { Text = $"Shard on {shard.Guilds.Count} servers, {shard.Guilds.Count(x => x.DownloadedMemberCount >= x.MemberCount)} have all users downloaded" }
            };

            embed.AddField("Discord", $"Gateway: {gateway}ms\nRest: {rest}ms", true);
            embed.AddField("Database", $"Latency: {database}ms\nQueries: {databaseQueries}/s", true);

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
