using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Bot;
using Qmmands;
using Utili.Extensions;
using Utili.Implementations;

namespace Utili.Commands
{
    [Group("VoteChannels", "VoteChannel", "Votes")]
    public class VoteChannelsCommands : DiscordGuildModuleBase
    {
        [Command("AddEmoji", "AddEmote")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        [RequireBotChannelPermissions(Permission.AddReactions)]
        [DefaultCooldown(2, 5)]
        public async Task AddEmoji(
            IEmoji emoji,
            [RequireBotParameterChannelPermissions(Permission.AddReactions)]
            ITextChannel channel = null)
        {
            channel ??= Context.Channel as ITextChannel;

            var row = (await VoteChannels.GetRowsAsync(Context.Guild.Id, channel.Id)).FirstOrDefault();
            if (row is null)
            {
                await Context.Channel.SendFailureAsync("Error", $"{channel.Mention} is not a votes channel");
                return;
            }

            var emojiLimit = await Premium.IsGuildPremiumAsync(Context.Guild.Id) ? 5 : 2;

            if (row.Emotes.Count >= emojiLimit)
            {
                await Context.Channel.SendFailureAsync("Error",
                    $"Your server can only have up to {emojiLimit} emojis per votes channel\nRemove emojis with the removeEmoji command");
                return;
            }

            if (row.Emotes.Contains(emoji.ToString()))
            {
                await Context.Channel.SendFailureAsync("Error", $"That emoji is already added to {channel.Mention}");
                return;
            }

            row.Emotes.Add(emoji.ToString());
            await VoteChannels.SaveRowAsync(row);

            await Context.Channel.SendSuccessAsync("Emote added", $"The {emoji} emoji was added to {channel.Mention}");
        }

        [Command("AddEmoji", "AddEmote")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        [RequireBotChannelPermissions(Permission.AddReactions)]
        public async Task AddEmoji(
            [RequireBotParameterChannelPermissions(Permission.AddReactions)]
            ITextChannel channel, 
            IEmoji emoji)
            => await AddEmoji(emoji, channel);

        [Command("RemoveEmoji", "RemoveEmote")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task RemoveEmoji(
            [Minimum(1)]
            int emojiNumber, 
            ITextChannel channel = null)
        {
            channel ??= Context.Channel as ITextChannel;
            
            var row = (await VoteChannels.GetRowsAsync(Context.Guild.Id, channel.Id)).FirstOrDefault();
            if (row is null)
            {
                await Context.Channel.SendFailureAsync("Error", $"{channel.Mention} is not a votes channel");
                return;
            }

            if (row.Emotes.Count < emojiNumber)
            {
                await Context.Channel.SendFailureAsync("Error", $"There are only {row.Emotes.Count} emojis for {channel.Mention}");
                return;
            }

            var removedEmoji = Context.Guild.GetEmoji(row.Emotes[emojiNumber - 1]);
            
            row.Emotes.RemoveAt(emojiNumber - 1);
            await VoteChannels.SaveRowAsync(row);
            
            await Context.Channel.SendSuccessAsync("Emoji removed", 
            $"The {removedEmoji} emoji was removed successfully");
        }

        [Command("RemoveEmoji", "RemoveEmote")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task RemoveEmoji(
            IEmoji emoji,
            ITextChannel channel = null)
        {
            channel ??= Context.Channel as ITextChannel;
            
            var row = (await VoteChannels.GetRowsAsync(Context.Guild.Id, channel.Id)).FirstOrDefault();
            if (row is null)
            {
                await Context.Channel.SendFailureAsync("Error", $"{channel.Mention} is not a votes channel");
                return;
            }
            
            if (row.Emotes.Any(x => x.ToString() == emoji.ToString()))
                await RemoveEmoji(row.Emotes.IndexOf(emoji.ToString()) + 1, channel);
            else
                await Context.Channel.SendFailureAsync("Error", $"{emoji} is not added to {channel.Mention}" +
                                                                "\nIf you can't specify the emoji, use its number found in the listEmoji command");
        }
        
        [Command("RemoveEmoji", "RemoveEmote")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task RemoveEmoji(
            ITextChannel channel,
            [Minimum(1)]
            int emojiNumber)
            => await RemoveEmoji(emojiNumber, channel);
        
        [Command("RemoveEmoji", "RemoveEmote")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task RemoveEmoji(
            ITextChannel channel,
            IEmoji emoji)
                => await RemoveEmoji(emoji, channel);
        
        [Command("ListEmojis", "ListEmoji", "ListEmotes", "ListEmote")]
        public async Task ListEmojis(ITextChannel channel = null)
        {
            channel ??= Context.Channel as ITextChannel;

            var row = (await VoteChannels.GetRowsAsync(Context.Guild.Id, channel.Id)).FirstOrDefault();
            if (row is null)
            {
                await Context.Channel.SendFailureAsync("Error", $"{channel.Mention} is not a votes channel");
                return;
            }
            
            var content = "";
            for (var i = 0; i < row.Emotes.Count; i++)
                content += $"{i + 1}: {row.Emotes[i]}\n";
            
            await Context.Channel.SendInfoAsync("Emojis", $"There are {row.Emotes.Count} emojis for {channel.Mention}\n\n{content}");
        }
    }
}
