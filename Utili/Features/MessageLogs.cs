using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database;
using Database.Data;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using static Utili.Program;
using static Utili.MessageSender;

namespace Utili.Features
{
    internal static class MessageLogs
    {
        public static async Task MessageReceived(SocketCommandContext context)
        {
            if (context.User.IsBot || context.Channel is SocketDMChannel) return;

            MessageLogsRow row = Database.Data.MessageLogs.GetRow(context.Guild.Id);

            if ((row.DeletedChannelId == 0 && row.EditedChannelId == 0) || row.ExcludedChannels.Contains(context.Channel.Id)) return;

            MessageLogsMessageRow message = new MessageLogsMessageRow
            {
                GuildId = context.Guild.Id,
                ChannelId = context.Channel.Id,
                MessageId = context.Message.Id,
                UserId = context.User.Id,
                Timestamp = DateTime.UtcNow,
                Content = EString.FromDecoded(context.Message.Content)
            };

            Database.Data.MessageLogs.SaveMessage(message);
            Database.Data.MessageLogs.DeleteOldMessages(context.Guild.Id, context.Channel.Id,
                Premium.IsPremium(context.Guild.Id));
        }

        public static async Task MessageEdited(SocketCommandContext context)
        {
            MessageLogsRow row = Database.Data.MessageLogs.GetRow(context.Guild.Id);
            if ((row.DeletedChannelId == 0 && row.EditedChannelId == 0) || row.ExcludedChannels.Contains(context.Channel.Id)) return;

            MessageLogsMessageRow message = Database.Data.MessageLogs.GetMessage(context.Guild.Id, context.Channel.Id, context.Message.Id);
            if (message == null) return;
            Embed embed = await GetEditedEmbedAsync(message, context);

            message.Content = EString.FromDecoded(context.Message.Content);
            Database.Data.MessageLogs.SaveMessage(message);

            SocketTextChannel channel = context.Guild.GetTextChannel(row.EditedChannelId);
            if(channel == null) return;
            await SendEmbedAsync(channel, embed);
        }

        public static async Task MessageDeleted(SocketGuild guild, SocketTextChannel channel, ulong messageId)
        {
            MessageLogsRow row = Database.Data.MessageLogs.GetRow(guild.Id);
            if ((row.DeletedChannelId == 0 && row.EditedChannelId == 0) || row.ExcludedChannels.Contains(channel.Id)) return;

            MessageLogsMessageRow message = Database.Data.MessageLogs.GetMessage(guild.Id, channel.Id, messageId);
            if(message == null) return;
            Embed embed = await GetDeletedEmbedAsync(channel, message);

            Database.Data.MessageLogs.DeleteMessagesById(new[] {message.Id});

            SocketTextChannel logChannel = guild.GetTextChannel(row.DeletedChannelId);
            if(logChannel == null) return;
            await SendEmbedAsync(logChannel, embed);
        }

        public static async Task MessagesBulkDeleted(SocketGuild guild, SocketTextChannel channel, List<ulong> messageIds)
        {
            MessageLogsRow row = Database.Data.MessageLogs.GetRow(guild.Id);
            if ((row.DeletedChannelId == 0 && row.EditedChannelId == 0) || row.ExcludedChannels.Contains(channel.Id)) return;

            List<MessageLogsMessageRow> messages =
                Database.Data.MessageLogs.GetMessages(guild.Id, channel.Id, messageIds.ToArray());

            Embed embed = await GetBulkDeletedEmbedAsync(channel, messages, messageIds.Count);

            Database.Data.MessageLogs.DeleteMessagesByMessageId(guild.Id, channel.Id, messageIds.ToArray());

            SocketTextChannel logChannel = guild.GetTextChannel(row.DeletedChannelId);
            if(logChannel == null) return;
            await SendEmbedAsync(logChannel, embed);
        }

        private static async Task<Embed> GetEditedEmbedAsync(MessageLogsMessageRow before, SocketCommandContext after)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(66, 182, 245);
            embed.WithDescription($"**Message by {after.User.Mention} edited in {(after.Channel as SocketTextChannel).Mention}** [Jump]({after.Message.GetJumpUrl()})");

            if(before.Content.Value.Length > 1024 || after.Message.Content.Length > 1024)
            {
                if(before.Content.Value.Length < 2024 - embed.Description.Length - 2)
                {
                    embed.Description += $"\n{before.Content.Value}";
                }
                else
                {
                    embed.Description += "\nThe message is too large to fit in this embed";
                }

                embed.WithFooter($"Message {before.MessageId}");
                embed.WithTimestamp(new DateTimeOffset(before.Timestamp));
            }
            else
            {
                embed.AddField("Before", before.Content.Value);
                embed.AddField("After", after.Message.Content);

                embed.WithFooter("Sent");
                embed.WithTimestamp(new DateTimeOffset(before.Timestamp));
            }

            string avatar = after.User.GetAvatarUrl();
            if (string.IsNullOrEmpty(avatar)) avatar = after.User.GetDefaultAvatarUrl();

            embed.WithAuthor(new EmbedAuthorBuilder
            {
                Name = $"{after.User.Username}#{after.User.Discriminator}",
                IconUrl = avatar
            });

            return embed.Build();
        }

        private static async Task<Embed> GetDeletedEmbedAsync(SocketTextChannel channel, MessageLogsMessageRow message)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(245, 66, 66);

            RestUser user = await _rest.GetUserAsync(message.UserId);
            string userMention = message.UserId.ToString();

            if (user != null)
            {
                userMention = user.Mention;

                string avatar = user.GetAvatarUrl();
                if (string.IsNullOrEmpty(avatar)) avatar = user.GetDefaultAvatarUrl();

                embed.WithAuthor(new EmbedAuthorBuilder
                {
                    Name = $"{user.Username}#{user.Discriminator}",
                    IconUrl = avatar
                });
            }

            embed.WithDescription($"**Message by {userMention} deleted in {channel.Mention}**");

            if(message.Content.Value.Length > 2024 - embed.Description.Length - 2)
            {
                embed.Description += "\nThe message is too large to fit in this embed";
            }
            else
            {
                embed.Description += $"\n{message.Content.Value}";
            }

            embed.WithFooter($"Message {message.MessageId}");
            embed.WithTimestamp(new DateTimeOffset(message.Timestamp));

            return embed.Build();
        }

        private static async Task<Embed> GetBulkDeletedEmbedAsync(SocketTextChannel channel, List<MessageLogsMessageRow> messages, int total)
        {
            EmbedBuilder embed = new EmbedBuilder();

            string paste = "Failed to upload message logs to haste server";
            try
            {
                paste = $"[View {messages.Count} of {total} logged]({await PasteMessagesAsync(messages, total)})";
            } 
            catch {}

            embed.WithColor(245, 66, 66);
            embed.WithDescription($"**{total} messages bulk deleted in {channel.Mention}**\n{paste}");

            embed.WithAuthor(new EmbedAuthorBuilder
            {
                Name = "Bulk deletion"
            });

            return embed.Build();
        }

        private static async Task<string> PasteMessagesAsync(List<MessageLogsMessageRow> messages, int total)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Messages {total}");
            sb.AppendLine($"Logged   {messages.Count}");
            sb.AppendLine();
            sb.AppendLine();

            List<RestUser> cachedUsers = new List<RestUser>();

            foreach (MessageLogsMessageRow message in messages)
            {
                RestUser user;
                if (cachedUsers.Any(x => x.Id == message.UserId))
                    user = cachedUsers.First(x => x.Id == message.UserId);
                else
                {
                    user = await _rest.GetUserAsync(message.UserId);
                    cachedUsers.Add(user);
                }

                if (user == null) sb.AppendLine($"{user.Id}");
                else sb.AppendLine($"{user} ({user.Id})");
                sb.AppendLine($" at {Helper.ToUniversalDateTime(message.Timestamp)} UTC");

                string messageContent = "    " + message.Content.Value.Replace("\n", "\n    ");

                sb.AppendLine($"{messageContent}\n");
            }

            string content = sb.ToString().TrimEnd('\r', '\n');
            return await Program._haste.PasteAsync(content, "txt");
        }
    }
}
