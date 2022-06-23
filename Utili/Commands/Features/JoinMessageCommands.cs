using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Database;
using Database.Extensions;
using Disqord.Rest;
using Qmmands;
using Utili.Extensions;
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
        public async Task PreviewAsync()
        {
            var config = await _dbContext.JoinMessageConfigurations.GetForGuildAsync(Context.GuildId);
            var message = JoinMessageService.GetJoinMessage(config, Context.Author);
            var sentMessage = await Context.Channel.SendMessageAsync(message);

            if (!config.CreateThread || !Context.Channel.BotHasPermissions(Permission.CreatePublicThreads)) return;
            var threadTitle = config.ThreadTitle.Replace("%user%", Context.Author.Name);
            await Bot.CreatePublicThreadAsync(Context.ChannelId, threadTitle, sentMessage.Id, options: new DefaultRestRequestOptions { Reason = "Join message" });
        }
    }
}
