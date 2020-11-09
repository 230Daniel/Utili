using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using static Utili.Program;
using static Utili.MessageSender;
using static Utili.Helper;

namespace Utili.Commands
{
    public class InfoCommands : ModuleBase<SocketCommandContext>
    {
        [Command("About"), Alias("Info"), Cooldown(3)]
        public async Task About()
        {
            DiscordSocketClient shard = GetShardForGuild(Context.Guild);

            string about = @$"By 230Daniel#1920
                            In {Database.Sharding.GetGuildCount()} servers
                            Shard {shard.ShardId} ({_totalShards} total)
                            [Website](https://{_config.Domain})
                            [Dashboard](https://{_config.Domain}/dashboard)
                            [Get Premium](https://{_config.Domain}/premium)
                            [Support & Requests Server](https://discord.gg/hCYWk9x)";

            await SendInfoAsync(Context.Channel, "Utili v2 Beta", about);
        }

        [Command("Help"), Alias("Commands")]
        public async Task Help()
        {
            string help = $@"[List of Commands](https://{_config.Domain}/commands)
                            [Dashboard](https://{_config.Domain}/dashboard/{Context.Guild.Id}/core)";

            await SendInfoAsync(Context.Channel, "Utili", help);
        }
    }
}
