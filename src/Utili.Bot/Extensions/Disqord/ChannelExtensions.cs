using Disqord;
using Disqord.Gateway;

namespace Utili.Bot.Extensions;

public static class ChannelExtensions
{
    public static bool BotHasPermissions(this IGuildChannel channel, Permissions permissions)
    {
        return channel.GetGuild().GetCurrentMember().CalculateChannelPermissions(channel).HasFlag(permissions);
    }

    public static IGuild GetGuild(this IGuildChannel channel)
    {
        return (channel.Client as DiscordClientBase).GetGuild(channel.GuildId);
    }
}
