using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Qmmands;
using Utili.Extensions;
using Utili.Implementations;

namespace Utili.Commands
{
    public class UtilCommands : DiscordGuildModuleBase
    {
        [Command("Prune", "Purge", "Clear"), DefaultCooldown(1, 10), RequireAuthorChannelPermissions(Permission.ManageMessages)]
        public async Task Prune([Remainder] string argsString = null)
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

            if(!Context.Channel.BotHasPermissions(Context.Bot, out string missingPermissions, Permission.ViewChannel, Permission.ReadMessageHistory, Permission.ManageMessages))
            {
                await Context.Channel.SendFailureAsync("Error", $"I'm missing the following permissions: {missingPermissions}");
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

            int pinned = messages.RemoveAll(x => x is IUserMessage y && y.IsPinned);
            int outdated = messages.RemoveAll(x => x.CreatedAt.UtcDateTime < DateTime.UtcNow - TimeSpan.FromDays(13.9));

            if (pinned == 1) content += $"{pinned} message was not deleted because it is pinned\n";
            else if (pinned > 1) content += $"{pinned} messages were not deleted because they are pinned\n";
            if (outdated == 1) content += $"{outdated} message was not deleted because it is older than 14 days\n";
            else if (outdated > 1) content += $"{outdated} messages were not deleted because they are older than 14 days\n";

            await Context.Channel.DeleteMessagesAsync(messages.Select(x => x.Id));

            string title = $"{messages.Count} messages deleted";
            if (messages.Count == 1) title = $"{messages.Count} message deleted";

            IUserMessage sentMessage = await Context.Channel.SendSuccessAsync(title, content);
            await Task.Delay(5000);
            await sentMessage.DeleteAsync();
        }

        [Command("React", "AddReaction", "AddEmoji"), DefaultCooldown(2, 5), RequireAuthorChannelPermissions(Permission.ManageMessages)]
        public async Task React(ulong messageId, [Remainder] string emojiString)
        {
            IMessage message = await Context.Channel.FetchMessageAsync(messageId);

            if (message is null)
            {
                await Context.Channel.SendFailureAsync("Error", $"No message was found in <#{Context.Channel.Id}> with ID {messageId}\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498)");
                return;
            }

            if (!Context.Channel.BotHasPermissions(Context.Bot, out string missingPermissions, Permission.ViewChannel, Permission.ReadMessageHistory, Permission.AddReactions))
            {
                await Context.Channel.SendFailureAsync("Error",
                    $"I'm missing the following permissions: {missingPermissions}");
                return;
            }

            IEmoji emoji = Helper.GetEmoji(emojiString, Context.Guild);

            try
            {
                await message.AddReactionAsync(emoji);
                await Context.Channel.SendSuccessAsync("Reaction added",
                    $"The {emojiString} reaction was added to a message sent by {message.Author.Mention}");
            }
            catch
            {
                await Context.Channel.SendFailureAsync("Error",
                    $"No emoji was found matching {emojiString}\nType the emoji itself in the command");
            }
        }

        [Command("React", "AddReaction", "AddEmoji"), DefaultCooldown(2, 5), RequireAuthorChannelPermissions(Permission.ManageMessages)]
        public async Task React(ITextChannel channel, ulong messageId, [Remainder] string emojiString)
        {
            // TODO: Check for perms in the specified channel (custom param attrib?)

            IMessage message = await channel.FetchMessageAsync(messageId);

            if (message is null)
            {
                await Context.Channel.SendFailureAsync("Error",
                    $"No message was found in <#{channel.Id}> with ID {messageId}\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");
                return;
            }

            if (!channel.BotHasPermissions(Context.Bot, out string missingPermissions, Permission.ViewChannel, Permission.ReadMessageHistory, Permission.AddReactions))
            {
                await Context.Channel.SendFailureAsync("Error", $"I'm missing the following permissions: {missingPermissions}");
                return;
            }

            IEmoji emoji = Helper.GetEmoji(emojiString, Context.Guild);

            try
            {
                await message.AddReactionAsync(emoji);

                await Context.Channel.SendSuccessAsync("Reaction added",
                    $"The {emojiString} reaction was added to a message sent by {message.Author.Mention} in {channel.Mention}");
            }
            catch
            {
                await Context.Channel.SendFailureAsync("Error",
                    $"No emoji was found matching {emojiString}\nType the emoji itself in the command");
            }
        }

        // TODO: Implement when member chunking is done

        //[Command("Random", "Pick")]
        //public async Task Random(ITextChannel channel = null, ulong messageId = 0, [Remainder] string emojiString = "")
        //{
        //    await Context.Guild.DownloadAndKeepUsersAsync(TimeSpan.FromMinutes(15));
        //    Random random = new Random();

        //    if (messageId == 0)
        //    {
        //        int index = random.Next(Context.Guild.Users.Count);
        //        SocketGuildUser user = Context.Guild.Users.ElementAt(index);

        //        await SendInfoAsync(Context.Channel, "Random user",
        //            $"{user.Mention}\nThis user was picked randomly from {Context.Guild.Users.Count} server member{(Context.Guild.Users.Count == 1 ? "" : "s")}.");
        //    }
        //    else
        //    {
        //        channel ??= Context.Channel as ITextChannel;

        //        IMessage message = await channel.GetMessageAsync(messageId);

        //        if (message is null)
        //        {
        //            await Context.Channel.SendFailureAsync("Error",
        //                $"No message was found in <#{channel.Id}> with ID {messageId}\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");
        //            return;
        //        }

        //        IEmote emoji = Helper.GetEmoji(emojiString, Context.Guild);

        //        if(message.Reactions.TryGetValue(emoji, out ReactionMetadata _))
        //        {
        //            List<IUser> users = (await message.GetReactionUsersAsync(emoji, 1000).FlattenAsync()).ToList();
        //            int index = random.Next(users.Count);
        //            IUser user = users.ElementAt(index);

        //            await SendInfoAsync(Context.Channel, "Random user",
        //                $"{user.Mention}\nThis user was picked randomly from {users.Count} user{(users.Count == 1 ? "" : "s")} that reacted to [this message]({message.GetJumpUrl()}) with {emojiString}.");
        //        }
        //    }
        //}

        //[Command("Random", "Pick")]
        //public async Task Random(ulong messageId = 0, [Remainder] string emojiString = "")
        //{
        //    await Random(Context.Channel as ITextChannel, messageId, emojiString);
        //}

        //[Command("WhoHas")]
        //public async Task WhoHas(IRole role, int page = 1)
        //{
        //    await Context.Guild.DownloadAndKeepUsersAsync(TimeSpan.FromMinutes(15));
        //    List<SocketGuildUser> users = Context.Guild.Users.OrderBy(x => x.Nickname ?? x.Username).Where(x => x.Roles.Any(y => y.Id == role.Id)).ToList();

        //    int totalPages = (int) Math.Ceiling(users.Count / 50d);
        //    users = users.Skip((page - 1) * 50).Take(50).ToList();
        //    if ((page < 1 || page > totalPages) && totalPages != 0)
        //    {
        //        await Context.Channel.SendFailureAsync("Error", "Invalid page number");
        //        return; 
        //    }
        //    if (totalPages == 0) page = 0;

        //    string output = users.Aggregate("", (current, user) => current + $"{user.Mention}\n");
        //    if (output == "") output = "There are no users with that role.";

        //    await SendInfoAsync(Context.Channel, $"Users with @{role.Name}", output, $"Page {page} of {totalPages}");
        //}

        [Command("B64Encode")]
        public async Task B64Encode([Remainder] string input)
        {
            string output = Helper.EncodeString(input);
            await Context.Channel.SendSuccessAsync("Encoded string to base 64", output);
        }

        [Command("B64Decode")]
        public async Task B64Decode([Remainder] string input)
        {
            string output = Helper.DecodeString(input);

            if (output == input)
            {
                await Context.Channel.SendFailureAsync("Failed to decode string", "The input string is not valid base 64");
                return;
            }

            await Context.Channel.SendSuccessAsync("Decoded string from base 64", output);
        }
    }
}
