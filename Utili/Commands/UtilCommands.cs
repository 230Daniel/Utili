﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Disqord.Rest.Default;
using Qmmands;
using Utili.Extensions;
using Utili.Implementations;

namespace Utili.Commands
{
    public class UtilCommands : DiscordGuildModuleBase
    {
        [Command("Prune", "Purge", "Clear")]
        [DefaultCooldown(1, 10)]
        [RequireAuthorChannelPermissions(Permission.ManageMessages)]
        [RequireBotChannelPermissions(Permission.ManageMessages | Permission.ReadMessageHistory)]
        public async Task Prune(
            [Remainder]
            string argsString = null)
        {
            if (string.IsNullOrEmpty(argsString))
            {
                await Context.Channel.SendInfoAsync("Prune",
                    "Add one or more of the following arguments in any order to delete messages\n" +
                    "[amount] - The amount of messages to delete (default 100)\n" +
                    "before [message id] - Only messages before a particular message\n" +
                    "after [message id] - Only messages after a particular message\n\n" +
                    "[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498)");

                return;
            }

            string[] args = argsString.Split(" ");

            uint count = 0;
            bool countSet = false;

            IMessage afterMessage = null;
            IMessage beforeMessage = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (uint.TryParse(args[i], out uint newCount))
                {
                    count = newCount;
                    countSet = true;
                }
                else
                {
                    switch (args[i].ToLower())
                    {
                        case "before":
                            try
                            {
                                i++;
                                ulong messageId = ulong.Parse(args[i]);
                                beforeMessage = await Context.Channel.FetchMessageAsync(messageId);
                                if (beforeMessage is null) throw new Exception();
                                break;
                            }
                            catch
                            {
                                await Context.Channel.SendFailureAsync("Error", "Invalid message ID after \"before\"\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");
                                return;
                            }

                        case "after":
                            try
                            {
                                i++;
                                ulong messageId = ulong.Parse(args[i]);
                                afterMessage = await Context.Channel.FetchMessageAsync(messageId);
                                if (afterMessage is null) throw new Exception();
                                break;
                            }
                            catch
                            {
                                await Context.Channel.SendFailureAsync("Error", "Invalid message ID after \"after\"\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");
                                return;
                            }
                            
                        default:
                            await Context.Channel.SendFailureAsync("Error", $"Invalid argument \"{args[i].ToLower()}\"");
                            return;
                    }
                }
            }

            if(afterMessage is not null && beforeMessage is not null && afterMessage.CreatedAt >= beforeMessage.CreatedAt)
            {
                await Context.Channel.SendFailureAsync("Error", "There are no messages between the after and before messages");
                return;
            }

            await Context.Message.DeleteAsync(new DefaultRestRequestOptions {Reason = $"Prune (manual by {Context.Message.Author} {Context.Message.Author.Id})"});
            Task delay = Task.Delay(800);

            string content = "";

            if(!countSet) count = 100;

            if (count > 1000)
            {
                count = 1000;
                content = "For premium servers, you can delete up to 1000 messages at once\n";
            }

            else if (count > 100 && !await Premium.IsGuildPremiumAsync(Context.Guild.Id))
            {
                count = 100;
                content = "For non-premium servers, you can delete up to 100 messages at once\n";
            }

            await delay;

            List<IMessage> messages;
            if(afterMessage is not null) messages = (await Context.Channel.FetchMessagesAsync((int)count, RetrievalDirection.After, afterMessage.Id)).ToList();
            else if (beforeMessage is not null) messages = (await Context.Channel.FetchMessagesAsync((int)count,RetrievalDirection.Before, beforeMessage.Id)).ToList();
            else messages = (await Context.Channel.FetchMessagesAsync((int)count)).ToList();

            messages = messages.OrderBy(x => x.CreatedAt.UtcDateTime).ToList();
            if (beforeMessage is not null)
            {
                if (messages.Any(x => x.Id == beforeMessage.Id))
                {
                    int index = messages.FindIndex(x => x.Id == beforeMessage.Id);
                    messages.RemoveRange(index, messages.Count - index);
                }
            }

            int pinned = messages.RemoveAll(x => x is IUserMessage {IsPinned: true});
            int outdated = messages.RemoveAll(x => x.CreatedAt.UtcDateTime < DateTime.UtcNow - TimeSpan.FromDays(13.9));

            if (pinned == 1) content += $"{pinned} message was not deleted because it is pinned\n";
            else if (pinned > 1) content += $"{pinned} messages were not deleted because they are pinned\n";
            if (outdated == 1) content += $"{outdated} message was not deleted because it is older than 14 days\n";
            else if (outdated > 1) content += $"{outdated} messages were not deleted because they are older than 14 days\n";

            await Context.Channel.DeleteMessagesAsync(messages.Select(x => x.Id), new DefaultRestRequestOptions {Reason = $"Prune (manual by {Context.Message.Author} {Context.Message.Author.Id})"});

            string title = $"{messages.Count} messages deleted";
            if (messages.Count == 1) title = $"{messages.Count} message deleted";

            IUserMessage sentMessage = await Context.Channel.SendSuccessAsync(title, content);
            await Task.Delay(5000);
            await sentMessage.DeleteAsync();
        }

        [Command("React", "AddReaction", "AddEmoji")]
        [DefaultCooldown(2, 5)]
        [RequireAuthorChannelPermissions(Permission.AddReactions | Permission.ManageMessages)]
        [RequireBotChannelPermissions(Permission.AddReactions | Permission.ReadMessageHistory)]
        public async Task React(
            ulong messageId,
            IEmoji emoji)
        {
            IMessage message = await Context.Channel.FetchMessageAsync(messageId);

            if (message is null)
            {
                await Context.Channel.SendFailureAsync("Error", $"No message was found in <#{Context.Channel.Id}> with ID {messageId}\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498)");
                return;
            }
            
            await message.AddReactionAsync(emoji);
            await Context.Channel.SendSuccessAsync("Reaction added",
                $"The {emoji} reaction was added to a message sent by {message.Author.Mention}");
        }

        [Command("React", "AddReaction", "AddEmoji")]
        [DefaultCooldown(2, 5)]
        public async Task React(
            [RequireAuthorParameterChannelPermissions(Permission.AddReactions | Permission.ManageMessages)]
            [RequireBotParameterChannelPermissions(Permission.AddReactions | Permission.ReadMessageHistory)]
            ITextChannel channel, 
            ulong messageId, 
            IEmoji emoji)
        {
            IMessage message = await channel.FetchMessageAsync(messageId);

            if (message is null)
            {
                await Context.Channel.SendFailureAsync("Error",
                    $"No message was found in <#{channel.Id}> with ID {messageId}\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");
                return;
            }

            await message.AddReactionAsync(emoji);

            await Context.Channel.SendSuccessAsync("Reaction added",
                $"The {emoji} reaction was added to a message sent by {message.Author.Mention} in {channel.Mention}");
        }

        [Command("Random", "Pick")]
        public async Task Random()
        {
            await Context.Channel.SendFailureAsync("Error",
                "Sorry, this command has been permanently disabled due to changes in the Discord API and scalability issues" +
                "\nPerhaps you're looking for `random [channel] [message id] [emoji]`?");
        }
        
        [Command("Random", "Pick")]
        public async Task Random(ITextChannel channel, ulong messageId, IEmoji emoji)
        {
            IMessage message = await channel.FetchMessageAsync(messageId);
            
            if (message is null || !message.Reactions.HasValue)
            {
                await Context.Channel.SendFailureAsync("Error",
                    $"No message was found in {channel.Mention} with ID {messageId}\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498)");
                return;
            }
            
            if(message.Reactions.Value.TryGetValue(emoji, out var reaction))
            {
                IReadOnlyList<IUser> reactedMembers = await message.FetchReactionsAsync(emoji, int.MaxValue);
                Random random = new();
                IUser member = reactedMembers[random.Next(0, reactedMembers.Count)];

                await Context.Channel.SendInfoAsync("Random member",
                    $"{member.Mention}\n" +
                    $"This member was picked randomly from {reactedMembers.Count} member{(reactedMembers.Count == 1 ? "" : "s")} " +
                    $"that reacted to [this message]({message.GetJumpUrl(Context.GuildId)}) with {emoji}.");
            }
            else
            {
                await Context.Channel.SendFailureAsync("Error",
                    $"That message doesn't have the {emoji} reaction");
            }
        }
        
        [Command("Random", "Pick")]
        public async Task Random(ulong messageId, IEmoji emoji)
        {
            await Random(Context.Channel, messageId, emoji);
        }
        
        [Command("WhoHas")]
        public async Task WhoHas()
        {
            await Context.Channel.SendFailureAsync("Error",
                "Sorry, this command has been permanently disabled due to changes in the Discord API and scalability issues");
        }

        [Command("B64Encode")]
        public async Task B64Encode([Remainder] string input)
        {
            string output = input.ToEncoded();
            await Context.Channel.SendSuccessAsync("Encoded string to base 64", output);
        }

        [Command("B64Decode")]
        public async Task B64Decode([Remainder] string input)
        {
            string output = input.ToDecoded();

            if (output == input)
            {
                await Context.Channel.SendFailureAsync("Failed to decode string", "The input string is not valid base 64");
                return;
            }

            await Context.Channel.SendSuccessAsync("Decoded string from base 64", output);
        }
    }
}
