using Disqord.Bot.Commands;
using Disqord.Gateway;

namespace Utili.Bot.Extensions;

public static class DiscordGuildCommandContextExtensions
{
    public static CachedMember GetCurrentMember(this IDiscordGuildCommandContext context)
    {
        return context.Bot.GetCurrentMember(context.GuildId);
    }

    public static CachedGuild GetGuild(this IDiscordGuildCommandContext context)
    {
        return context.Bot.GetGuild(context.GuildId);
    }

    public static CachedMessageGuildChannel GetChannel(this IDiscordGuildCommandContext context)
    {
        return context.Bot.GetMessageGuildChannel(context.GuildId, context.ChannelId);
    }
}
