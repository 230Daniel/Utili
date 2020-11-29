using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Data;
using Discord;
using Discord.Commands;
using static Utili.Program;
using static Utili.MessageSender;
using Utili.Commands;

namespace Utili.Features
{
    internal static class VoteChannels
    {
        public static async Task MessageReceived(SocketCommandContext context)
        {
            if (BotPermissions.IsMissingPermissions(context.Channel, new[] {ChannelPermission.AddReactions}, out _))
            {
                return;
            }

            List<VoteChannelsRow> rows = Database.Data.VoteChannels.GetRows(context.Guild.Id, context.Channel.Id);

            if (rows.Count == 0)
            {
                return;
            }

            VoteChannelsRow row = rows.First();

            if (!DoesMessageObeyRule(context, row))
            {
                return;
            }

            List<IEmote> emotes = row.Emotes;

            if (Premium.IsPremium(context.Guild.Id))
            {
                emotes = emotes.Take(5).ToList();
            }
            else
            {
                emotes = emotes.Take(2).ToList();
            }

            await context.Message.AddReactionsAsync(emotes.ToArray());
        }

        public static bool DoesMessageObeyRule(SocketCommandContext context, VoteChannelsRow row)
        {
            return row.Mode switch
            {
                // All
                0 => true,

                // Images
                1 => MessageFilter.IsImage(context),

                // Videos
                2 => MessageFilter.IsVideo(context),

                // Media
                3 => MessageFilter.IsImage(context) || MessageFilter.IsVideo(context),

                // Music
                4 => MessageFilter.IsMusic(context) || MessageFilter.IsVideo(context),

                // Attachments
                5 => MessageFilter.IsAttachment(context),

                // URLs
                6 => MessageFilter.IsUrl(context),

                // URLs or Media
                7 => MessageFilter.IsImage(context) || MessageFilter.IsVideo(context) || MessageFilter.IsUrl(context),

                // Default
                _ => false,
            };
        }
    }

    [Group("VoteChannels"), Alias("Votes", "VoteChannel")]
    public class VoteChannelsCommands : ModuleBase<SocketCommandContext>
    {
        [Command("AddEmote"), Alias("AddEmotes", "AddEmoji", "AddEmojis")] [Permission(Perm.ManageGuild)] [Cooldown(2)]
        public async Task Add(string emoteString, ITextChannel channel = null)
            {
                if(channel == null) channel = Context.Channel as ITextChannel;

                if (BotPermissions.IsMissingPermissions(channel,
                    new[] {ChannelPermission.AddReactions}, out string missingPermissions))
                {
                    await SendFailureAsync(Context.Channel, "Error",
                        $"I'm missing the following permissions: {missingPermissions}");
                    return;
                }

                List<VoteChannelsRow> rows = Database.Data.VoteChannels.GetRows(Context.Guild.Id, channel.Id);
                if (rows.Count == 0)
                {
                    await SendFailureAsync(Context.Channel, "Error",
                        $"{channel.Mention} is not a votes channel");
                    return;
                }

                VoteChannelsRow row = rows.First();

                int limit = 2;
                if (Premium.IsPremium(Context.Guild.Id)) limit = 5;

                if(row.Emotes.Count >= limit)
                {
                    await SendFailureAsync(Context.Channel, "Error",
                        $"Your server can only have up to {limit} emotes per votes channel\nRemove emotes with the removeemote command");
                    return;
                }

                IEmote emote = Helper.GetEmote(emoteString);

                if (row.Emotes.Contains(emote))
                {
                    await SendFailureAsync(Context.Channel, "Error",
                        "That emote is already added");
                    return;
                }

                try
                {
                    await Context.Message.AddReactionAsync(emote);
                }
                catch
                {
                    await SendFailureAsync(Context.Channel, "Error",
                        $"An emote was not found matching {emoteString}");
                    return;
                }

                await Context.Message.RemoveReactionAsync(emote, _client.CurrentUser);

                row.Emotes.Add(emote);
                Database.Data.VoteChannels.SaveRow(row);

                await SendSuccessAsync(Context.Channel, "Emote added", 
                    $"The {emote} emote was added successfully");
            }

        [Command("AddEmote"), Alias("AddEmotes", "AddEmoji", "AddEmojis")]
        public async Task Add(ITextChannel channel, string emoteString)
        {
            await Add(emoteString, channel);
        }

        [Command("RemoveEmote"), Alias("RemoveEmotes", "RemoveEmoji", "RemoveEmojis")] [Permission(Perm.ManageGuild)] [Cooldown(2)]
        public async Task Remove(string emoteString, ITextChannel channel = null)
        {
            if(channel == null) channel = Context.Channel as ITextChannel;

            List<VoteChannelsRow> rows = Database.Data.VoteChannels.GetRows(Context.Guild.Id, channel.Id);
            if (rows.Count == 0)
            {
                await SendFailureAsync(Context.Channel, "Error",
                    $"{channel.Mention} is not a votes channel");
                return;
            }

            VoteChannelsRow row = rows.First();

            IEmote emote = Helper.GetEmote(emoteString);

            if (!row.Emotes.Contains(emote))
            {
                if (int.TryParse(emoteString, out int index))
                {
                    try
                    {
                        index--;
                        emote = row.Emotes[index];
                    }
                    catch
                    {
                        await SendFailureAsync(Context.Channel, "Error",
                            "That emote is not added\nIf you can't specify the emote, use its number found in the listemote command");
                        return;
                    }
                }
                else
                {
                    await SendFailureAsync(Context.Channel, "Error",
                        "That emote is not added\nIf you can't specify the emote, use its number found in the listemote command");
                    return;
                }
            }

            row.Emotes.Remove(emote);
            Database.Data.VoteChannels.SaveRow(row);

            await SendSuccessAsync(Context.Channel, "Emote removed", 
                $"The {emote} emote was removed successfully");
        }

        [Command("RemoveEmote"), Alias("RemoveEmotes", "RemoveEmoji", "RemoveEmojis")]
        public async Task Remove(ITextChannel channel, string emoteString)
        {
            await Remove(emoteString, channel);
        }

        [Command("ListEmote"), Alias("ListEmotes", "ListEmoji", "ListEmojis")]
        public async Task List(ITextChannel channel = null)
        {
            if (channel == null) channel = Context.Channel as ITextChannel;

            List<VoteChannelsRow> rows = Database.Data.VoteChannels.GetRows(Context.Guild.Id, channel.Id);
            if (rows.Count == 0)
            {
                await SendFailureAsync(Context.Channel, "Error",
                    $"{channel.Mention} is not a votes channel");
                return;
            }

            VoteChannelsRow row = rows.First();
            string content = "";

            for (int i = 0; i < row.Emotes.Count; i++)
            {
                content += $"{i + 1}: {row.Emotes[i]}\n";
            }

            await SendInfoAsync(Context.Channel, "Emotes",
                $"There are {row.Emotes.Count} emotes for {channel.Mention}\n\n{content}");
        }
    }
}
