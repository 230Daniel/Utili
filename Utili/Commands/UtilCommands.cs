using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using static Utili.MessageSender;

namespace Utili.Commands
{
    public class UtilCommands : ModuleBase<SocketCommandContext>
    {
        [Command("Prune"), Alias("Purge", "Clear")]
        public async Task Prune([Remainder] string argsString = null)
        {
            if (string.IsNullOrEmpty(argsString))
            {
                await SendInfoAsync(Context.Channel, "Prune",
                    "Add one or more of the following arguments in any order to delete messages\n" +
                    "[amount] - The amount of messages to delete (default 100)\n" +
                    "before [message id] - Only messages before a particular message\n" +
                    "after [message id] - Only messages after a particular message\n\n" +
                    "[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");
                return;
            }

            SocketTextChannel channel = Context.Channel as SocketTextChannel;

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
                                beforeMessage = await Context.Channel.GetMessageAsync(messageId);
                                if (beforeMessage == null) throw new Exception();
                                break;
                            }
                            catch
                            {
                                await SendFailureAsync(channel, "Error", "Invalid message ID after \"before\"\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");
                                return;
                            }

                        case "after":
                            try
                            {
                                i++;
                                ulong messageId = ulong.Parse(args[i]);
                                afterMessage = await Context.Channel.GetMessageAsync(messageId);
                                if (afterMessage == null) throw new Exception();
                                break;
                            }
                            catch
                            {
                                await SendFailureAsync(channel, "Error", "Invalid message ID after \"after\"\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");
                                return;
                            }
                            
                        default:
                            await SendFailureAsync(channel, "Error", $"Invalid argument \"{args[i].ToLower()}\"");
                            return;
                    }
                }
            }

            if(afterMessage != null && beforeMessage != null && afterMessage.Timestamp >= beforeMessage.Timestamp)
            {
                await SendFailureAsync(channel, "Error", "There are no messages between the after and before messages");
                return;
            }

            await Context.Message.DeleteAsync();
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
            if(afterMessage != null) messages = (await channel.GetMessagesAsync(afterMessage.Id, Direction.After, (int)count).FlattenAsync()).ToList();
            else if (beforeMessage != null) messages = (await channel.GetMessagesAsync(beforeMessage, Direction.Before, (int)count).FlattenAsync()).ToList();
            else messages = (await channel.GetMessagesAsync((int)count).FlattenAsync()).ToList();

            messages = messages.OrderBy(x => x.Timestamp.UtcDateTime).ToList();
            if (beforeMessage != null)
            {
                if (messages.Any(x => x.Id == beforeMessage.Id))
                {
                    int index = messages.FindIndex(x => x.Id == beforeMessage.Id);
                    messages.RemoveRange(index, messages.Count - index);
                }
            }

            int pinned = messages.RemoveAll(x => x.IsPinned);
            int outdated = messages.RemoveAll(x => x.CreatedAt.UtcDateTime < DateTime.UtcNow - TimeSpan.FromDays(13.9));

            if (pinned == 1) content += $"{pinned} message was not deleted because it is pinned";
            else if (pinned > 1) content += $"{pinned} messages were not deleted because they are pinned";
            if (outdated == 1) content += $"{outdated} message was not deleted because it is older than 14 days";
            else if (outdated > 1) content += $"{outdated} messages were not deleted because they are older than 14 days";

            await channel.DeleteMessagesAsync(messages);

            string title = $"{messages.Count} messages deleted";
            if (messages.Count == 1) title = $"{messages.Count} message deleted";

            RestUserMessage sentMessage = await SendSuccessAsync(Context.Channel, title, content);
            await Task.Delay(5000);
            await sentMessage.DeleteAsync();
        }

        [Command("React"), Alias("AddReaction", "AddEmoji"), Cooldown(2), Permission(Perm.ManageMessages)]
        public async Task React(ulong messageId, [Remainder] string emojiString)
        {
            IMessage message = await Context.Channel.GetMessageAsync(messageId);

            if (message == null)
            {
                await SendFailureAsync(Context.Channel, "Error",
                    $"No message was found in <#{Context.Channel.Id}> with ID {messageId}\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");
                return;
            }

            IEmote emoji = Helper.GetEmote(emojiString);

            try
            {
                await message.AddReactionAsync(emoji);

                await SendSuccessAsync(Context.Channel, "Reaction added",
                    $"The {emojiString} reaction was added to a message sent by {message.Author.Mention}");
            }
            catch
            {
                await SendFailureAsync(Context.Channel, "Error",
                    $"No emoji was found matching {emojiString}\nType the emoji itself in the command");
            }
        }

        [Command("React"), Alias("AddReaction", "AddEmoji"), Cooldown(2), Permission(Perm.ManageMessages)]
        public async Task React(SocketTextChannel channel, ulong messageId, [Remainder] string emojiString)
        {
            IMessage message = await channel.GetMessageAsync(messageId);

            if (message == null)
            {
                await SendFailureAsync(Context.Channel, "Error",
                    $"No message was found in <#{Context.Channel.Id}> with ID {messageId}\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");
                return;
            }

            IEmote emoji = Helper.GetEmote(emojiString);

            try
            {
                await message.AddReactionAsync(emoji);

                await SendSuccessAsync(Context.Channel, "Reaction added",
                    $"The {emojiString} reaction was added to a message sent by {message.Author.Mention} in {channel.Mention}");
            }
            catch
            {
                await SendFailureAsync(Context.Channel, "Error",
                    $"No emoji was found matching {emojiString}\nType the emoji itself in the command");
            }
        }

        [Command("WhoHas"), Cooldown(3)]
        public async Task WhoHas(SocketRole role, int page = 1)
        {
            if (!Context.Guild.HasAllMembers)
            {
                await Context.Guild.DownloadUsersAsync();
            }

            List<SocketGuildUser> users = Context.Guild.Users.OrderBy(x => x.Nickname ?? x.Username).Where(x => x.Roles.Any(y => y.Id == role.Id)).ToList();

            int totalPages = (int) Math.Ceiling(users.Count / 50d);
            users = users.Skip((page - 1) * 50).Take(50).ToList();
            if ((page < 1 || page > totalPages) && totalPages != 0)
            {
                await SendFailureAsync(Context.Channel, "Error", "Invalid page number");
                return; 
            }
            if (totalPages == 0) page = 0;

            string output = users.Aggregate("", (current, user) => current + $"{user.Mention}\n");
            if (output == "") output = "There are no users with those roles.";

            await SendInfoAsync(Context.Channel, $"Users with @{role.Name}", output, $"Page {page} of {totalPages}");
        }

        [Command("B64Encode")]
        public async Task B64Encode([Remainder] string input)
        {
            string output = Helper.EncodeString(input);
            await SendSuccessAsync(Context.Channel, "Encoded string to base 64", output);
        }

        [Command("B64Decode")]
        public async Task B64Decode([Remainder] string input)
        {
            string output = Helper.DecodeString(input);

            if (output == input)
            {
                await SendFailureAsync(Context.Channel, "Failed to decode string", "The input string is not valid base 64");
                return;
            }

            await SendSuccessAsync(Context.Channel, "Decoded string from base 64", output);
        }
    }
}
