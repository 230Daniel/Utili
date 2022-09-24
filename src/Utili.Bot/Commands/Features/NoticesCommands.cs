using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Utili.Database;
using Utili.Database.Extensions;
using Qmmands;
using Qmmands.Text;
using Utili.Bot.Implementations;
using Utili.Bot.Services;

namespace Utili.Bot.Commands.Features;

[TextGroup("notice", "notices")]
public class NoticesCommands : MyDiscordTextGuildModuleBase
{
    private readonly DatabaseContext _dbContext;

    public NoticesCommands(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    [TextCommand("preview", "send")]
    [RequireNotThread]
    [RequireBotPermissions(Permissions.SendMessages | Permissions.SendEmbeds | Permissions.SendAttachments)]
    public async Task<IResult> PreviewAsync()
    {
        var config = await _dbContext.NoticeConfigurations.GetForGuildChannelAsync(Context.GuildId, Context.ChannelId);
        if (config is null) return Failure("Error", "This channel does not have a notice.");
        return Response(NoticesService.GetNotice(config));
    }

    [TextCommand("preview", "send")]
    [RequireBotPermissions(Permissions.SendMessages | Permissions.SendEmbeds | Permissions.SendAttachments)]
    public async Task<IResult> PreviewAsync(
        [RequireAuthorParameterChannelPermissions(Permissions.ViewChannels | Permissions.ReadMessageHistory)]
        ITextChannel channel)
    {
        var config = await _dbContext.NoticeConfigurations.GetForGuildChannelAsync(Context.GuildId, channel.Id);
        if (config is null) return Failure("Error", $"{channel.Mention} does not have a notice.");
        return Response(NoticesService.GetNotice(config));
    }
}
