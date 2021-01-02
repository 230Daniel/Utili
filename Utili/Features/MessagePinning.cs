﻿using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Utili.Commands;
using static Utili.MessageSender;
using Database.Data;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;

namespace Utili.Features
{
    public class MessagePinningCommands : ModuleBase<SocketCommandContext>
    {
        private async Task Pin(ulong messageId, SocketTextChannel pinChannel, SocketTextChannel channel)
        {
            IUserMessage message = null;
            try { message = await channel.GetMessageAsync(messageId) as IUserMessage; } catch { }

            if (message == null)
            {
                await SendFailureAsync(Context.Channel, "Error",
                    $"No message was found in <#{Context.Channel.Id}> with ID {messageId}\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");
                return;
            }

            MessagePinningRow row = await MessagePinning.GetRowAsync(Context.Guild.Id);

            if(row.Pin) try { await message.PinAsync(); } catch { }

            if (pinChannel == null)
            {
                try { pinChannel = Context.Guild.GetTextChannel(row.PinChannelId); } catch { }
            }

            if (pinChannel == null && row.Pin)
            {
                await SendSuccessAsync(Context.Channel, "Message pinned",
                    "Set a pin channel on the dashboard or specify one in the command if you want the message to be copied to another channel as well.");
            }
            else if (pinChannel == null && !row.Pin)
            {
                await SendFailureAsync(Context.Channel, "Error",
                    "Pinning is not enabled on this server.");
            }
            else
            {
                if (BotPermissions.IsMissingPermissions(channel, new[] {ChannelPermission.ManageWebhooks}, out string missingPermissions))
                {
                    await SendFailureAsync(Context.Channel, "Error",
                        $"I'm missing the following permissions: {missingPermissions}");
                    return;
                }

                RestWebhook webhook = null;
                try { webhook = await pinChannel.GetWebhookAsync(row.WebhookIds.First(x => x.Item1 == pinChannel.Id).Item2); } catch { }

                if (webhook == null)
                {
                    FileStream avatar = File.OpenRead("Avatar.png");
                    webhook = await pinChannel.CreateWebhookAsync("Utili Message Pinning", avatar);
                    avatar.Close();

                    if (row.WebhookIds.Any(x => x.Item1 == pinChannel.Id))
                    {
                        row.WebhookIds[row.WebhookIds.FindIndex(x => x.Item1 == pinChannel.Id)] = (pinChannel.Id, webhook.Id);
                    }
                    else row.WebhookIds.Add((pinChannel.Id, webhook.Id));
                    await MessagePinning.SaveRowAsync(row);
                }

                string username = $"{message.Author} in #{channel}";
                string avatarUrl = message.Author.GetAvatarUrl();
                if (string.IsNullOrEmpty(avatarUrl)) avatarUrl = message.Author.GetDefaultAvatarUrl();

                DiscordWebhookClient webhookClient = new DiscordWebhookClient(webhook);
                if(!(string.IsNullOrEmpty(message.Content) && message.Embeds.Count == 0)) await webhookClient.SendMessageAsync(message.Content, false, message.Embeds.Select(x => x as Embed), username, avatarUrl);

                foreach (IAttachment attachment in message.Attachments)
                {
                    WebRequest request = WebRequest.Create(attachment.Url);
                    Stream stream = (await request.GetResponseAsync()).GetResponseStream();
                    await webhookClient.SendFileAsync(stream, attachment.Filename, "", username: username, avatarUrl: avatarUrl);
                    stream.Close();
                }

                await SendSuccessAsync(Context.Channel, "Message pinned",
                    $"The message was sent to {pinChannel.Mention}");
            }
        }

        [Command("Pin")] [Permission(Perm.ManageMessages), Cooldown(3)]
        public async Task Pin(ulong messageId, SocketTextChannel pinChannel = null)
        {
            await Pin(messageId, pinChannel, Context.Channel as SocketTextChannel);
        }

        [Command("Pin")] [Permission(Perm.ManageMessages), Cooldown(3)]
        public async Task Pin(SocketTextChannel channel, ulong messageId, SocketTextChannel pinChannel = null)
        {
            await Pin(messageId, pinChannel, channel);
        }
    }
}