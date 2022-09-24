using System.Threading.Tasks;
using Disqord;
using Utili.Database;
using Utili.Database.Extensions;
using Disqord.Rest;
using Qmmands.Text;
using Utili.Bot.Services;
using Utili.Bot.Extensions;
using Utili.Bot.Implementations;

namespace Utili.Bot.Commands;

[TextGroup("joinmessage", "joinmessages")]
public class JoinMessageCommands : MyDiscordTextGuildModuleBase
{
    private readonly DatabaseContext _dbContext;

    public JoinMessageCommands(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    [TextCommand("preview")]
    [RequireNotThread]
    [RequireNotVoice]
    public async Task PreviewAsync()
    {
        var config = await _dbContext.JoinMessageConfigurations.GetForGuildAsync(Context.GuildId);
        var message = JoinMessageService.GetJoinMessage(config, Context.Author);
        var sentMessage = await Context.GetChannel().SendMessageAsync(message);

        if (!config.CreateThread || !Context.GetChannel().BotHasPermissions(Permissions.CreatePublicThreads)) return;
        var threadTitle = config.ThreadTitle;
        if (string.IsNullOrWhiteSpace(threadTitle)) threadTitle = "Welcome %user%";
        threadTitle = threadTitle.Replace("%user%", Context.Author.Name);
        await Bot.CreatePublicThreadAsync(Context.ChannelId, threadTitle, sentMessage.Id, options: new DefaultRestRequestOptions { Reason = "Join message" });
    }
}
