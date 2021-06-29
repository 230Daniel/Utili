using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using Utili.Extensions;

namespace Utili.Services
{
    public class MessageLogsService
    {
        private readonly ILogger<MessageLogsService> _logger;
        private readonly DiscordClientBase _client;
        private readonly HasteService _haste;

        public MessageLogsService(ILogger<MessageLogsService> logger, DiscordClientBase client, HasteService haste)
        {
            _logger = logger;
            _client = client;
            _haste = haste;
        }

        public async Task MessageReceived(MessageReceivedEventArgs e)
        {
            try
            {
                if(e.Message.Author.IsBot || !e.GuildId.HasValue) return;

                var row = await MessageLogs.GetRowAsync(e.GuildId.Value);
                if ((row.DeletedChannelId == 0 && row.EditedChannelId == 0) || row.ExcludedChannels.Contains(e.ChannelId)) return;

                var message = new MessageLogsMessageRow()
                {
                    GuildId = e.GuildId.Value,
                    ChannelId = e.ChannelId,
                    MessageId = e.MessageId,
                    UserId = e.Message.Author.Id,
                    Timestamp = e.Message.CreatedAt().UtcDateTime,
                    Content = EString.FromDecoded(e.Message.Content)
                };

                await MessageLogs.SaveMessageAsync(message);
                await MessageLogs.DeleteOldMessagesAsync(e.GuildId.Value, e.ChannelId, await Premium.IsGuildPremiumAsync(e.GuildId.Value));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message received ({Guild}/{Channel}/{Message})", e.GuildId, e.ChannelId, e.MessageId);
            }
        }

        public async Task MessageUpdated(MessageUpdatedEventArgs e)
        {
            try
            {
                if (!e.GuildId.HasValue) return;

                ITextChannel channel = _client.GetTextChannel(e.GuildId.Value, e.ChannelId);

                var row = await MessageLogs.GetRowAsync(e.GuildId.Value);
                if ((row.DeletedChannelId == 0 && row.EditedChannelId == 0) || 
                    row.ExcludedChannels.Contains(e.ChannelId)) return;

                var previousMessage = await MessageLogs.GetMessageAsync(e.GuildId.Value, e.ChannelId, e.MessageId);
                if (previousMessage is null || !e.Model.Content.HasValue || e.Model.Content.Value == previousMessage.Content.Value) return;

                var newMessage = e.NewMessage ?? await channel.FetchMessageAsync(e.MessageId) as IUserMessage;
                var embed = GetEditedEmbed(newMessage, previousMessage);

                previousMessage.Content = EString.FromDecoded(e.Model.Content.Value);
                await MessageLogs.SaveMessageAsync(previousMessage);

                ITextChannel logChannel = _client.GetTextChannel(e.GuildId.Value, row.EditedChannelId);
                if (logChannel is not null) await logChannel.SendEmbedAsync(embed);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message updated");
            }
        }

        public async Task MessageDeleted(MessageDeletedEventArgs e)
        {
            try
            {
                if (!e.GuildId.HasValue) return;

                var row = await MessageLogs.GetRowAsync(e.GuildId.Value);
                if ((row.DeletedChannelId == 0 && row.EditedChannelId == 0) ||
                    row.ExcludedChannels.Contains(e.ChannelId)) return;

                var message =
                    await MessageLogs.GetMessageAsync(e.GuildId.Value, e.ChannelId, e.MessageId);
                if (message is null) return;

                var member = _client.GetMember(e.GuildId.Value, message.UserId) ?? await _client.FetchMemberAsync(e.GuildId.Value, message.UserId);
                if (member is not null && member.IsBot) return;

                var embed = GetDeletedEmbed(message, member);

                await MessageLogs.DeleteMessagesAsync(e.GuildId.Value, e.ChannelId, new[] {e.MessageId.RawValue});

                ITextChannel logChannel = _client.GetTextChannel(e.GuildId.Value, row.DeletedChannelId);
                if (logChannel is not null) await logChannel.SendEmbedAsync(embed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message deleted");
            }
        }

        public async Task MessagesDeleted(MessagesDeletedEventArgs e)
        {
            try
            {
                var row = await MessageLogs.GetRowAsync(e.GuildId);
                if ((row.DeletedChannelId == 0 && row.EditedChannelId == 0) ||
                    row.ExcludedChannels.Contains(e.ChannelId)) return;

                var messages = await MessageLogs.GetMessagesAsync(e.GuildId, e.ChannelId, e.MessageIds.Select(x => x.RawValue).ToArray());

                var embed = GetBulkDeletedEmbed(e, await PasteMessagesAsync(messages, e.MessageIds.Count), messages);
                
                await MessageLogs.DeleteMessagesAsync(e.GuildId, e.ChannelId, e.MessageIds.Select(x => x.RawValue).ToArray());

                ITextChannel logChannel = _client.GetTextChannel(e.GuildId, row.DeletedChannelId);
                if (logChannel is not null) await logChannel.SendEmbedAsync(embed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on messages deleted");
            }
        }

        private LocalEmbed GetEditedEmbed(IUserMessage newMessage, MessageLogsMessageRow previousMessage)
        {
            var builder = new LocalEmbed()
                .WithColor(new Color(66, 182, 245))
                .WithDescription($"**Message by {newMessage.Author.Mention} edited in {Mention.TextChannel(newMessage.ChannelId)}** [Jump]({newMessage.GetJumpUrl(previousMessage.GuildId)})")
                .WithAuthor(newMessage.Author)
                .WithFooter($"Message {previousMessage.MessageId}")
                .WithTimestamp(DateTime.SpecifyKind(previousMessage.Timestamp, DateTimeKind.Utc));

            if (previousMessage.Content.Value.Length > 1024 || newMessage.Content.Length > 1024)
            {
                if (previousMessage.Content.Value.Length < 2024 - builder.Description.Length - 2)
                    builder.Description += $"\n{previousMessage.Content.Value}";
                else
                    builder.Description += "\nThe message is too large to fit in this embed";
            }
            else
            {
                builder.AddField("Before", previousMessage.Content.Value);
                builder.AddField("After", newMessage.Content);
            }

            return builder;
        }

        private LocalEmbed GetDeletedEmbed(MessageLogsMessageRow deletedMessage, IMember member)
        {
            var builder = new LocalEmbed()
                .WithColor(new Color(245, 66, 66))
                .WithDescription($"**Message by {Mention.User(deletedMessage.UserId)} deleted in {Mention.TextChannel(deletedMessage.ChannelId)}**")
                .WithFooter($"Message {deletedMessage.MessageId}")
                .WithTimestamp(DateTime.SpecifyKind(deletedMessage.Timestamp, DateTimeKind.Utc));

            if (member is null) builder.WithAuthor("Unknown member");
            else builder.WithAuthor(member);

            if (deletedMessage.Content.Value.Length > 2024 - builder.Description.Length - 2)
                builder.Description += "\nThe message is too large to fit in this embed";
            else
                builder.Description += $"\n{deletedMessage.Content.Value}";

            return builder;
        }

        private LocalEmbed GetBulkDeletedEmbed(MessagesDeletedEventArgs e, string paste, List<MessageLogsMessageRow> messages)
        {
            var builder = new LocalEmbed()
                .WithColor(new Color(245, 66, 66))
                .WithDescription($"**{e.MessageIds.Count} messages bulk deleted in {Mention.TextChannel(e.ChannelId)}**\n" +
                                 $"[View {messages.Count} logged message{(messages.Count == 1 ? "" : "s")}]({paste})")
                .WithAuthor("Bulk Deletion");

            return builder;
        }

        private async Task<string> PasteMessagesAsync(List<MessageLogsMessageRow> messages, int total)
        {
            try
            {
                var sb = new StringBuilder();

                sb.AppendLine($"Messages {total}");
                sb.AppendLine($"Logged   {messages.Count}");
                sb.AppendLine();
                sb.AppendLine();

                var cachedUsers = new Dictionary<Snowflake, IUser>();

                foreach (var message in messages)
                {
                    if (!cachedUsers.TryGetValue(message.UserId, out var user))
                    {
                        user = await _client.FetchUserAsync(message.UserId);
                        cachedUsers.Add(message.UserId, user);
                        await Task.Delay(500);
                    }

                    if (user is null) sb.AppendLine($"Unknown member ({user.Id})");
                    else sb.AppendLine($"{user} ({user.Id})");
                    sb.AppendLine($" at {message.Timestamp.ToUniversalFormat()} UTC");

                    var messageContent = "    " + message.Content.Value.Replace("\n", "\n    ");

                    sb.AppendLine($"{messageContent}\n");
                }

                var content = sb.ToString().TrimEnd('\r', '\n');
                return await _haste.PasteAsync(content, "txt");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception thrown uploading messages to Haste server");
                return "Failed to upload messages to haste server";
            }
        }
    }
}
