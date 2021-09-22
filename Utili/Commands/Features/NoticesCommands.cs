using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Database;
using Database.Extensions;
using Qmmands;
using Utili.Extensions;
using Utili.Services;

namespace Utili.Commands.Features
{
    [Group("Notice", "Notices")]
    public class NoticesCommands : DiscordGuildModuleBase
    {
        private readonly DatabaseContext _dbContext;
        
        public NoticesCommands(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        [Command("Preview", "Send")]
        [RequireNotThread]
        [RequireBotChannelPermissions(Permission.SendMessages | Permission.SendEmbeds | Permission.SendAttachments)]
        public async Task Preview()
        {
            var config = await _dbContext.NoticeConfigurations.GetForGuildChannelAsync(Context.GuildId, Context.Channel.Id);
            if (config is null) await Context.Channel.SendFailureAsync("Error", "This channel does not have a notice.");
            else await Context.Channel.SendMessageAsync(NoticesService.GetNotice(config));
        }
        
        [Command("Preview", "Send")]
        [RequireBotChannelPermissions(Permission.SendMessages | Permission.SendEmbeds | Permission.SendAttachments)]
        public async Task Preview(
            [RequireAuthorParameterChannelPermissions(Permission.ViewChannels | Permission.ReadMessageHistory)]
            ITextChannel channel)
        {
            var config = await _dbContext.NoticeConfigurations.GetForGuildChannelAsync(Context.GuildId, channel.Id);
            if (config is null) await Context.Channel.SendFailureAsync("Error", $"{channel.Mention} does not have a notice.");
            else await Context.Channel.SendMessageAsync(NoticesService.GetNotice(config));
        }
    }
}
