using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Qmmands;
using Utili.Implementations;
using Utili.Services;

namespace Utili.Commands.Features
{
    [Group("Notice", "Notices")]
    public class NoticesCommands : DiscordGuildModuleBase
    {
        [Command("Preview", "Send")]
        [RequireBotChannelPermissions(Permission.SendMessages | Permission.EmbedLinks | Permission.AttachFiles)]
        public async Task Preview(
            [RequireAuthorParameterChannelPermissions(Permission.ViewChannel | Permission.ReadMessageHistory)]
            ITextChannel channel = null)
        {
            channel ??= Context.Channel;
            var row = await Notices.GetRowAsync(Context.Guild.Id, channel.Id);
            await Context.Channel.SendMessageAsync(NoticesService.GetNotice(row));
        }
    }
}
