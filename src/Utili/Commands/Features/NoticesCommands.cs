using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Database;
using Database.Extensions;
using Qmmands;
using Utili.Implementations;
using Utili.Services;

namespace Utili.Commands.Features
{
    [Group("notice", "notices")]
    public class NoticesCommands : MyDiscordGuildModuleBase
    {
        private readonly DatabaseContext _dbContext;

        public NoticesCommands(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Command("preview", "send")]
        [RequireNotThread]
        [RequireBotChannelPermissions(Permission.SendMessages | Permission.SendEmbeds | Permission.SendAttachments)]
        public async Task<DiscordCommandResult> PreviewAsync()
        {
            var config = await _dbContext.NoticeConfigurations.GetForGuildChannelAsync(Context.GuildId, Context.Channel.Id);
            if (config is null) return Failure("Error", "This channel does not have a notice.");
            return Response(NoticesService.GetNotice(config));
        }

        [Command("preview", "send")]
        [RequireBotChannelPermissions(Permission.SendMessages | Permission.SendEmbeds | Permission.SendAttachments)]
        public async Task<DiscordCommandResult> PreviewAsync(
            [RequireAuthorParameterChannelPermissions(Permission.ViewChannels | Permission.ReadMessageHistory)]
            ITextChannel channel)
        {
            var config = await _dbContext.NoticeConfigurations.GetForGuildChannelAsync(Context.GuildId, channel.Id);
            if (config is null) return Failure("Error", $"{channel.Mention} does not have a notice.");
            return Response(NoticesService.GetNotice(config));
        }
    }
}
