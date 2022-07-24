using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Utili.Database;
using Utili.Database.Extensions;
using Qmmands;
using Qmmands.Text;
using Utili.Bot.Implementations;
using Utili.Bot.Extensions;
using Utili.Bot.Services;

namespace Utili.Bot.Commands;

[TextGroup("votechannels", "votechannel", "votes")]
public class VoteChannelsCommands : MyDiscordTextGuildModuleBase
{
    private readonly DatabaseContext _dbContext;
    private readonly IsPremiumService _isPremiumService;

    public VoteChannelsCommands(DatabaseContext dbContext, IsPremiumService isPremiumService)
    {
        _dbContext = dbContext;
        _isPremiumService = isPremiumService;
    }

    [TextCommand("addemoji", "addemote")]
    [RequireAuthorPermissions(Permissions.ManageGuild)]
    [DefaultRateLimit(2, 5)]
    public async Task<IResult> AddEmojiAsync(
        IEmoji emoji,
        [RequireBotParameterChannelPermissions(Permissions.AddReactions)]
        ITextChannel channel)
    {
        channel ??= Context.Channel as ITextChannel;

        var config = await _dbContext.VoteChannelConfigurations.GetForGuildChannelAsync(Context.GuildId, channel.Id);
        if (config is null)
            return Failure("Error", $"{channel.Mention} is not a votes channel");

        var emojiLimit = await _isPremiumService.GetIsGuildPremiumAsync(Context.GuildId) ? 5 : 2;

        if (config.Emojis.Count >= emojiLimit)
            return Failure("Error",
                $"Your server can only have up to {emojiLimit} emojis per votes channel\nRemove emojis with the removeEmoji command");

        if (config.Emojis.Contains(emoji.ToString()))
            return Failure("Error", $"That emoji is already added to {channel.Mention}");

        config.Emojis.Add(emoji.ToString());
        _dbContext.VoteChannelConfigurations.Update(config);
        await _dbContext.SaveChangesAsync();

        return Success("Emoji added", $"The {emoji} emoji was added to {channel.Mention}");
    }

    [TextCommand("addemoji", "addemote")]
    [RequireAuthorPermissions(Permissions.ManageGuild)]
    public Task<IResult> AddEmojiAsync(
        [RequireBotParameterChannelPermissions(Permissions.AddReactions)]
        ITextChannel channel,
        IEmoji emoji)
        => AddEmojiAsync(emoji, channel);

    [TextCommand("addemoji", "addemote")]
    [RequireNotThread]
    [RequireAuthorPermissions(Permissions.ManageGuild)]
    [RequireBotPermissions(Permissions.AddReactions)]
    public Task<IResult> AddEmojiAsync(
        IEmoji emoji)
        => AddEmojiAsync(emoji, Context.Channel as ITextChannel);

    [TextCommand("removeemoji", "removeemote")]
    [RequireAuthorPermissions(Permissions.ManageGuild)]
    public async Task<IResult> RemoveEmojiAsync(
        [Minimum(1)] int emojiNumber,
        ITextChannel channel)
    {
        var config = await _dbContext.VoteChannelConfigurations.GetForGuildChannelAsync(Context.GuildId, channel.Id);
        if (config is null)
            return Failure("Error", $"{channel.Mention} is not a votes channel");

        if (config.Emojis.Count < emojiNumber)
            return Failure("Error", $"There are only {config.Emojis.Count} emojis for {channel.Mention}");

        var removedEmoji = Context.GetGuild().GetEmoji(config.Emojis[emojiNumber - 1]);

        config.Emojis.RemoveAt(emojiNumber - 1);
        _dbContext.VoteChannelConfigurations.Update(config);
        await _dbContext.SaveChangesAsync();

        return Success("Emoji removed",
            $"The {removedEmoji} emoji was removed successfully");
    }

    [TextCommand("removeemoji", "removeemote")]
    [RequireAuthorPermissions(Permissions.ManageGuild)]
    public async Task<IResult> RemoveEmojiAsync(
        IEmoji emoji,
        ITextChannel channel)
    {
        var config = await _dbContext.VoteChannelConfigurations.GetForGuildChannelAsync(Context.GuildId, channel.Id);
        if (config is null)
            return Failure("Error", $"{channel.Mention} is not a votes channel");

        if (config.Emojis.Any(x => x.ToString() == emoji.ToString()))
            return await RemoveEmojiAsync(config.Emojis.IndexOf(emoji.ToString()) + 1, channel);

        return Failure("Error", $"{emoji} is not added to {channel.Mention}" +
                                "\nIf you can't specify the emoji, use its number found in the listEmoji command");
    }

    [TextCommand("removeemoji", "removeemote")]
    [RequireAuthorPermissions(Permissions.ManageGuild)]
    public Task<IResult> RemoveEmojiAsync(
        ITextChannel channel,
        [Minimum(1)] int emojiNumber)
        => RemoveEmojiAsync(emojiNumber, channel);

    [TextCommand("removeemoji", "removeemote")]
    [RequireAuthorPermissions(Permissions.ManageGuild)]
    public Task<IResult> RemoveEmojiAsync(
        ITextChannel channel,
        IEmoji emoji)
        => RemoveEmojiAsync(emoji, channel);

    [TextCommand("removeemoji", "removeemote")]
    [RequireNotThread]
    [RequireAuthorPermissions(Permissions.ManageGuild)]
    public Task<IResult> RemoveEmojiAsync(
        IEmoji emoji)
        => RemoveEmojiAsync(emoji, Context.Channel as ITextChannel);

    [TextCommand("removeemoji", "removeemote")]
    [RequireNotThread]
    [RequireAuthorPermissions(Permissions.ManageGuild)]
    public Task<IResult> RemoveEmojiAsync(
        [Minimum(1)] int emojiNumber)
        => RemoveEmojiAsync(emojiNumber, Context.Channel as ITextChannel);

    [TextCommand("listemojis", "listemoji", "listemotes", "listemote")]
    public async Task<IResult> ListEmojisAsync(ITextChannel channel)
    {
        var config = await _dbContext.VoteChannelConfigurations.GetForGuildChannelAsync(Context.GuildId, channel.Id);
        if (config is null)
            return Failure("Error", $"{channel.Mention} is not a votes channel");

        var content = "";
        for (var i = 0; i < config.Emojis.Count; i++)
            content += $"{i + 1}: {config.Emojis[i]}\n";

        return Info("Emojis", $"There are {config.Emojis.Count} emojis for {channel.Mention}\n\n{content}");
    }

    [TextCommand("listemojis", "listemoji", "listemotes", "listemote")]
    [RequireNotThread]
    public Task<IResult> ListEmojisAsync()
        => ListEmojisAsync(Context.Channel as ITextChannel);
}
