using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Bot;
using Qmmands;
using Utili.Services;

namespace Utili.Commands
{
    [Group("JoinMessage", "JoinMessages")]
    public class JoinMessageCommands : DiscordGuildModuleBase
    {
        [Command("Preview")]
        public async Task Preview()
        {
            var row = await JoinMessage.GetRowAsync(Context.GuildId);
            var message = JoinMessageService.GetJoinMessage(row, Context.Message.Author as IMember);
            await Response(message);
        }
    }
}
