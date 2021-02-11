using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                return;

            List<VoteChannelsRow> rows = await Database.Data.VoteChannels.GetRowsAsync(context.Guild.Id, context.Channel.Id);
            if (rows.Count == 0) return;
            VoteChannelsRow row = rows.First();
            List<IEmote> emotes = row.Emotes;

            if (!DoesMessageObeyRule(context, row))
                return;

            if (await Premium.IsGuildPremiumAsync(context.Guild.Id))
                emotes = emotes.Take(5).ToList();
            else
                emotes = emotes.Take(2).ToList();

            await context.Message.AddReactionsAsync(emotes.ToArray());
        }

        private static bool DoesMessageObeyRule(SocketCommandContext context, VoteChannelsRow row)
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

                // Embeds
                8 => MessageFilter.IsEmbed(context),

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
            channel ??= Context.Channel as ITextChannel;

            if (BotPermissions.IsMissingPermissions(channel,
                new[] {ChannelPermission.AddReactions}, out string missingPermissions))
            {
                await SendFailureAsync(Context.Channel, "Error",
                    $"I'm missing the following permissions: {missingPermissions}");
                return;
            }

            List<VoteChannelsRow> rows = await Database.Data.VoteChannels.GetRowsAsync(Context.Guild.Id, channel.Id);
            if (rows.Count == 0)
            {
                await SendFailureAsync(Context.Channel, "Error",
                    $"{channel.Mention} is not a votes channel");
                return;
            }

            VoteChannelsRow row = rows.First();

            int limit = 2;
            if (await Premium.IsGuildPremiumAsync(Context.Guild.Id)) limit = 5;

            if(row.Emotes.Count >= limit)
            {
                await SendFailureAsync(Context.Channel, "Error",
                    $"Your server can only have up to {limit} emojis per votes channel\nRemove emojis with the removeEmoji command");
                return;
            }

            IEmote emote = Helper.GetEmote(emoteString, Context.Guild);
            try
            {
                await Context.Message.AddReactionAsync(emote);
            }
            catch
            {
                await SendFailureAsync(Context.Channel, "Error", $"An emoji was not found matching {emoteString}");
                return;
            }
            
            _ = Task.Run(async () =>
            {
                // Rate limit is 1 per 250ms. Stupid but what can you do?
                await Task.Delay(500);
                await Context.Message.RemoveReactionAsync(emote, _client.CurrentUser);
            });

            if (row.Emotes.Contains(emote))
            {
                await SendFailureAsync(Context.Channel, "Error", "That emoji is already added");
                return;
            }

            row.Emotes.Add(emote);
            await Database.Data.VoteChannels.SaveRowAsync(row);

            await SendSuccessAsync(Context.Channel, "Emote added", 
                $"The {emote} emoji was added successfully");
        }

        [Command("AddEmote"), Alias("AddEmotes", "AddEmoji", "AddEmojis")]
        public async Task Add(ITextChannel channel, string emoteString)
        {
            await Add(emoteString, channel);
        }

        [Command("RemoveEmote"), Alias("RemoveEmotes", "RemoveEmoji", "RemoveEmojis")] [Permission(Perm.ManageGuild)] [Cooldown(2)]
        public async Task Remove(string emoteString, ITextChannel channel = null)
        {
            channel ??= Context.Channel as ITextChannel;

            List<VoteChannelsRow> rows = await Database.Data.VoteChannels.GetRowsAsync(Context.Guild.Id, channel.Id);
            if (rows.Count == 0)
            {
                await SendFailureAsync(Context.Channel, "Error",
                    $"{channel.Mention} is not a votes channel");
                return;
            }

            VoteChannelsRow row = rows.First();
            IEmote emote = Helper.GetEmote(emoteString, Context.Guild);

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
                            "That emote is not added\nIf you can't specify the emoji, use its number found in the listEmoji command");
                        return;
                    }
                }
                else
                {
                    await SendFailureAsync(Context.Channel, "Error",
                        "That emote is not added\nIf you can't specify the emoji, use its number found in the listEmoji command");
                    return;
                }
            }

            row.Emotes.Remove(emote);
            await Database.Data.VoteChannels.SaveRowAsync(row);

            await SendSuccessAsync(Context.Channel, "Emoji removed", 
                $"The {emote} emoji was removed successfully");
        }

        [Command("RemoveEmote"), Alias("RemoveEmotes", "RemoveEmoji", "RemoveEmojis")]
        public async Task Remove(ITextChannel channel, string emoteString)
        {
            await Remove(emoteString, channel);
        }

        [Command("ListEmote"), Alias("ListEmotes", "ListEmoji", "ListEmojis")]
        public async Task List(ITextChannel channel = null)
        {
            channel ??= Context.Channel as ITextChannel;

            List<VoteChannelsRow> rows = await Database.Data.VoteChannels.GetRowsAsync(Context.Guild.Id, channel.Id);
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

            await SendInfoAsync(Context.Channel, "Emojis",
                $"There are {row.Emotes.Count} emojis for {channel.Mention}\n\n{content}");
        }
    }
}
