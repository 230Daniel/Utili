using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using static Utili.MessageSender;

namespace Utili.Commands
{
    public class UtilCommands : ModuleBase<SocketCommandContext>
    {
        [Command("Prune"), Alias("Purge", "Clear"), Cooldown(10), Permission(Perm.ManageMessages)]
        public async Task Prune(int count)
        {
            if (BotPermissions.IsMissingPermissions(Context.Channel, new[] {
                    ChannelPermission.ViewChannel, 
                    ChannelPermission.ReadMessageHistory,
                    ChannelPermission.ManageMessages},
                out string missingPermissions))
            {
                await SendFailureAsync(Context.Channel, "Error",
                    $"I'm missing the following permissions: {missingPermissions}");
            }

            await Context.Message.DeleteAsync();

            string content = "";

            if (count > 1000)
            {
                count = 1000;
                content = "For premium servers, you can delete up to 1000 messages at once\n";
            }

            else if (!Premium.IsPremium(Context.Guild.Id) && count > 200)
            {
                count = 100;
                content = "For non-premium servers, you can delete up to 100 messages at once\n";
            }

            List<IMessage> messages = (await Context.Channel.GetMessagesAsync(count).FlattenAsync()).ToList();
            int pinned = messages.RemoveAll(x => x.IsPinned);
            int outdated = messages.RemoveAll(x => x.CreatedAt.UtcDateTime < DateTime.UtcNow - TimeSpan.FromDays(13.9));

            if (pinned == 1)
            {
                content += $"{pinned} message was not deleted because it is pinned";
            }
            else if (pinned > 1)
            {
                content += $"{pinned} messages were not deleted because they are pinned";
            }

            if (outdated == 1)
            {
                content += $"{outdated} message was not deleted because it is older than 14 days";
            }
            else if (outdated > 1)
            {
                content += $"{outdated} messages were not deleted because they are older than 14 days";
            }
            
            SocketTextChannel channel = Context.Channel as SocketTextChannel;

            await channel.DeleteMessagesAsync(messages);

            string title = $"{messages.Count} messages deleted";
            if (messages.Count == 1)
            {
                title = $"{messages.Count} message deleted";
            }

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

            IEmote emoji = Helper.GetEmote(emojiString, Context.Guild);

            if (emoji == null)
            {
                await SendFailureAsync(Context.Channel, "Error",
                    $"No emoji was found matching {emojiString}\nType the emoji itself in the command\nEmojis from other servers don't work");
                return;
            }

            await message.AddReactionAsync(emoji);

            await SendSuccessAsync(Context.Channel, "Reaction added",
                $"The {emojiString} reaction was added to a message sent by {message.Author.Mention}");
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
