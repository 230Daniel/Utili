using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Bot;
using Qmmands;
using Utili.Features;
using Utili.Services;

namespace Utili.Commands
{
    [Group("JoinMessage", "JoinMessages")]
    public class JoinMessageCommands : DiscordGuildModuleBase
    {
        [Command("Preview")]
        public async Task Preview()
        {
            JoinMessageRow row = await JoinMessage.GetRowAsync(Context.GuildId);
            LocalMessage message = JoinMessageService.GetJoinMessage(row, Context.Message.Author as IMember);
            await Response(message);
        }
    }
}
