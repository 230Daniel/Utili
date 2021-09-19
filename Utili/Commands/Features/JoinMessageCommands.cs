using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Database;
using Database.Extensions;
using Qmmands;
using Utili.Services;

namespace Utili.Commands
{
    [Group("JoinMessage", "JoinMessages")]
    public class JoinMessageCommands : DiscordGuildModuleBase
    {
        private readonly DatabaseContext _dbContext;
        
        public JoinMessageCommands(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        [Command("Preview")]
        public async Task Preview()
        {
            var config = await _dbContext.JoinMessageConfigurations.GetForGuildAsync(Context.GuildId);
            var message = JoinMessageService.GetJoinMessage(config, Context.Message.Author as IMember);
            await Response(message);
        }
    }
}
