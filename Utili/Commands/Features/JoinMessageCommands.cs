using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Database;
using Database.Extensions;
using Qmmands;
using Utili.Services;

namespace Utili.Commands
{
    [Group("joinmessage", "joinmessages")]
    public class JoinMessageCommands : DiscordGuildModuleBase
    {
        private readonly DatabaseContext _dbContext;
        
        public JoinMessageCommands(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        [Command("preview")]
        public async Task<DiscordCommandResult> PreviewAsync()
        {
            var config = await _dbContext.JoinMessageConfigurations.GetForGuildAsync(Context.GuildId);
            var message = JoinMessageService.GetJoinMessage(config, Context.Message.Author as IMember);
            return Response(message);
        }
    }
}
