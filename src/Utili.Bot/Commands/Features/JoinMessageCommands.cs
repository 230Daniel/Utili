using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Utili.Database;
using Utili.Database.Extensions;
using Disqord.Rest;
using Qmmands;
using Utili.Bot.Services;
using Utili.Bot.Extensions;

namespace Utili.Bot.Commands
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
            var threadTitle = config.ThreadTitle;
            if (string.IsNullOrWhiteSpace(threadTitle)) threadTitle = "Welcome %user%";
            threadTitle = threadTitle.Replace("%user%", Context.Author.Name);
            await Bot.CreatePublicThreadAsync(Context.ChannelId, threadTitle, sentMessage.Id, options: new DefaultRestRequestOptions { Reason = "Join message" });
        }
    }
}
