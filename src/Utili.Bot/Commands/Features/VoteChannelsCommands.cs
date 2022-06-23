using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Utili.Database;
using Utili.Database.Extensions;
using Qmmands;
using Utili.Bot.Implementations;
using Utili.Bot.Extensions;

namespace Utili.Bot.Commands
{
    [Group("votechannels", "votechannel", "votes")]
    public class VoteChannelsCommands : MyDiscordGuildModuleBase
    {
        private readonly DatabaseContext _dbContext;

        public VoteChannelsCommands(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Command("addemoji", "addemote")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        [DefaultCooldown(2, 5)]
        public async Task<DiscordCommandResult> AddEmojiAsync(
            IEmoji emoji,
            [RequireBotParameterChannelPermissions(Permission.AddReactions)]
            ITextChannel channel)
        {
            channel ??= Context.Channel as ITextChannel;

            var config = await _dbContext.VoteChannelConfigurations.GetForGuildChannelAsync(Context.GuildId, channel.Id);
            if (config is null)
                return Failure("Error", $"{channel.Mention} is not a votes channel");

            var emojiLimit = await _dbContext.GetIsGuildPremiumAsync(Context.GuildId) ? 5 : 2;

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

        [Command("addemoji", "addemote")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public Task<DiscordCommandResult> AddEmojiAsync(
            [RequireBotParameterChannelPermissions(Permission.AddReactions)]
            ITextChannel channel,
            IEmoji emoji)
            => AddEmojiAsync(emoji, channel);

        [Command("addemoji", "addemote")]
        [RequireNotThread]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        [RequireBotChannelPermissions(Permission.AddReactions)]
        public Task<DiscordCommandResult> AddEmojiAsync(
            IEmoji emoji)
            => AddEmojiAsync(emoji, Context.Channel as ITextChannel);

        [Command("removeemoji", "removeemote")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task<DiscordCommandResult> RemoveEmojiAsync(
            [Minimum(1)] int emojiNumber,
            ITextChannel channel)
        {
            var config = await _dbContext.VoteChannelConfigurations.GetForGuildChannelAsync(Context.GuildId, channel.Id);
            if (config is null)
                return Failure("Error", $"{channel.Mention} is not a votes channel");

            if (config.Emojis.Count < emojiNumber)
                return Failure("Error", $"There are only {config.Emojis.Count} emojis for {channel.Mention}");

            var removedEmoji = Context.Guild.GetEmoji(config.Emojis[emojiNumber - 1]);

            config.Emojis.RemoveAt(emojiNumber - 1);
            _dbContext.VoteChannelConfigurations.Update(config);
            await _dbContext.SaveChangesAsync();

            return Success("Emoji removed",
                $"The {removedEmoji} emoji was removed successfully");
        }

        [Command("removeemoji", "removeemote")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task<DiscordCommandResult> RemoveEmojiAsync(
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

        [Command("removeemoji", "removeemote")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public Task<DiscordCommandResult> RemoveEmojiAsync(
            ITextChannel channel,
            [Minimum(1)] int emojiNumber)
            => RemoveEmojiAsync(emojiNumber, channel);

        [Command("removeemoji", "removeemote")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public Task<DiscordCommandResult> RemoveEmojiAsync(
            ITextChannel channel,
            IEmoji emoji)
            => RemoveEmojiAsync(emoji, channel);

        [Command("removeemoji", "removeemote")]
        [RequireNotThread]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public Task<DiscordCommandResult> RemoveEmojiAsync(
            IEmoji emoji)
            => RemoveEmojiAsync(emoji, Context.Channel as ITextChannel);

        [Command("removeemoji", "removeemote")]
        [RequireNotThread]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public Task<DiscordCommandResult> RemoveEmojiAsync(
            [Minimum(1)] int emojiNumber)
            => RemoveEmojiAsync(emojiNumber, Context.Channel as ITextChannel);

        [Command("listemojis", "listemoji", "listemotes", "listemote")]
        public async Task<DiscordCommandResult> ListEmojisAsync(ITextChannel channel)
        {
            var config = await _dbContext.VoteChannelConfigurations.GetForGuildChannelAsync(Context.GuildId, channel.Id);
            if (config is null)
                return Failure("Error", $"{channel.Mention} is not a votes channel");

            var content = "";
            for (var i = 0; i < config.Emojis.Count; i++)
                content += $"{i + 1}: {config.Emojis[i]}\n";

            return Info("Emojis", $"There are {config.Emojis.Count} emojis for {channel.Mention}\n\n{content}");
        }

        [Command("listemojis", "listemoji", "listemotes", "listemote")]
        [RequireNotThread]
        public Task<DiscordCommandResult> ListEmojisAsync()
            => ListEmojisAsync(Context.Channel as ITextChannel);
    }
}
