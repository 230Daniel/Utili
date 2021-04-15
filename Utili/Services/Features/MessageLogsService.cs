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
        ILogger<MessageLogsService> _logger;
        DiscordClientBase _client;
        HasteService _haste;

        public MessageLogsService(ILogger<MessageLogsService> logger, DiscordClientBase client, HasteService haste)
        {
            _logger = logger;
            _client = client;
            _haste = haste;
        }

        public Task MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if(e.Message.Author.IsBot || !e.GuildId.HasValue) return;

                    MessageLogsRow row = await Database.Data.MessageLogs.GetRowAsync(e.GuildId.Value);
                    if ((row.DeletedChannelId == 0 && row.EditedChannelId == 0) || row.ExcludedChannels.Contains(e.ChannelId)) return;

                    MessageLogsMessageRow message = new MessageLogsMessageRow
                    {
                        GuildId = e.GuildId.Value,
                        ChannelId = e.ChannelId,
                        MessageId = e.MessageId,
                        UserId = e.Message.Author.Id,
                        Timestamp = e.Message.CreatedAt.UtcDateTime,
                        Content = EString.FromDecoded(e.Message.Content)
                    };

                    await MessageLogs.SaveMessageAsync(message);
                    await MessageLogs.DeleteOldMessagesAsync(e.GuildId.Value, e.ChannelId, await Premium.IsGuildPremiumAsync(e.GuildId.Value));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception thrown on message received");
                }
            });
            return Task.CompletedTask;
        }

        public Task MessageUpdated(object sender, MessageUpdatedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (!e.GuildId.HasValue) return;

                    ITextChannel channel = _client.GetTextChannel(e.GuildId.Value, e.ChannelId);
                    IUserMessage newMessage = e.NewMessage ?? await channel.FetchMessageAsync(e.MessageId) as IUserMessage;
                    if(newMessage is null || newMessage.Author.IsBot) return;

                    MessageLogsRow row = await MessageLogs.GetRowAsync(e.GuildId.Value);
                    if ((row.DeletedChannelId == 0 && row.EditedChannelId == 0) || 
                        row.ExcludedChannels.Contains(e.ChannelId)) return;

                    MessageLogsMessageRow message = await MessageLogs.GetMessageAsync(e.GuildId.Value, e.ChannelId, e.MessageId);
                    if (message is null || message.Content.Value == newMessage.Content) return;

                    LocalEmbedBuilder embed = GetEditedEmbed(message, newMessage);

                    message.Content = EString.FromDecoded(newMessage.Content);
                    await MessageLogs.SaveMessageAsync(message);

                    ITextChannel logChannel = _client.GetTextChannel(e.GuildId.Value, row.EditedChannelId);
                    if (logChannel is not null) await logChannel.SendEmbedAsync(embed);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Exception thrown on message updated");
                }
            });
            return Task.CompletedTask;
        }

        public Task MessageDeleted(object sender, MessageDeletedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (!e.GuildId.HasValue) return;

                    MessageLogsRow row = await MessageLogs.GetRowAsync(e.GuildId.Value);
                    if ((row.DeletedChannelId == 0 && row.EditedChannelId == 0) ||
                        row.ExcludedChannels.Contains(e.ChannelId)) return;

                    MessageLogsMessageRow message =
                        await MessageLogs.GetMessageAsync(e.GuildId.Value, e.ChannelId, e.MessageId);
                    if (message is null) return;

                    IMember member = await _client.GetGuild(e.GuildId.Value).FetchMemberAsync(message.UserId);
                    if (member is not null && member.IsBot) return;

                    LocalEmbedBuilder embed = GetDeletedEmbed(message, e, member);

                    await MessageLogs.DeleteMessagesAsync(e.GuildId.Value, e.ChannelId, new[] {e.MessageId.RawValue});

                    ITextChannel logChannel = _client.GetTextChannel(e.GuildId.Value, row.DeletedChannelId);
                    if (logChannel is not null) await logChannel.SendEmbedAsync(embed);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception thrown on message deleted");
                }
            });
            return Task.CompletedTask;
        }

        public Task MessagesDeleted(object sender, MessagesDeletedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    MessageLogsRow row = await MessageLogs.GetRowAsync(e.GuildId);
                    if ((row.DeletedChannelId == 0 && row.EditedChannelId == 0) ||
                        row.ExcludedChannels.Contains(e.ChannelId)) return;

                    List<MessageLogsMessageRow> messages = await MessageLogs.GetMessagesAsync(e.GuildId, e.ChannelId, e.MessageIds.Select(x => x.RawValue).ToArray());

                    LocalEmbedBuilder embed = GetBulkDeletedEmbed(messages, e, await PasteMessagesAsync(messages, e.MessageIds.Count));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception thrown on messages deleted");
                }
            });
            return Task.CompletedTask;
        }

        LocalEmbedBuilder GetEditedEmbed(MessageLogsMessageRow previousMessage, IUserMessage newMessage)
        {
            LocalEmbedBuilder builder = new LocalEmbedBuilder()
                .WithColor(new Color(66, 182, 245))
                .WithDescription($"**Message by {newMessage.Author.Mention} edited in {newMessage.GetChannel(previousMessage.GuildId).Mention}** [Jump]({newMessage.GetJumpUrl(previousMessage.GuildId)})")
                .WithAuthor(newMessage.Author)
                .WithFooter($"ID {previousMessage.MessageId}")
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

        LocalEmbedBuilder GetDeletedEmbed(MessageLogsMessageRow deletedMessage, MessageDeletedEventArgs e, IMember member)
        {
            LocalEmbedBuilder builder = new LocalEmbedBuilder()
                .WithColor(new Color(245, 66, 66))
                .WithDescription($"**Message by {Mention.User(deletedMessage.UserId)} deleted in {Mention.TextChannel(deletedMessage.ChannelId)}**")
                .WithFooter($"ID {e.MessageId}")
                .WithTimestamp(DateTime.SpecifyKind(deletedMessage.Timestamp, DateTimeKind.Utc));

            if (member is null) builder.WithAuthor("Unknown member");
            else builder.WithAuthor(member);

            if (deletedMessage.Content.Value.Length > 2024 - builder.Description.Length - 2)
                builder.Description += "\nThe message is too large to fit in this embed";
            else
                builder.Description += $"\n{deletedMessage.Content.Value}";

            return builder;
        }

        LocalEmbedBuilder GetBulkDeletedEmbed(List<MessageLogsMessageRow> messages, MessagesDeletedEventArgs e, string paste)
        {
            LocalEmbedBuilder builder = new LocalEmbedBuilder()
                .WithColor(new Color(245, 66, 66))
                .WithDescription($"**{e.MessageIds.Count} messages bulk deleted in {Mention.TextChannel(e.ChannelId)}**\n{paste}")
                .WithAuthor("Bulk deletion");

            return builder;
        }

        async Task<string> PasteMessagesAsync(List<MessageLogsMessageRow> messages, int total)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"Messages {total}");
                sb.AppendLine($"Logged   {messages.Count}");
                sb.AppendLine();
                sb.AppendLine();

                Dictionary<Snowflake, IUser> cachedUsers = new Dictionary<Snowflake, IUser>();

                foreach (MessageLogsMessageRow message in messages)
                {
                    if (!cachedUsers.TryGetValue(message.UserId, out IUser user))
                    {
                        user = await _client.FetchUserAsync(message.UserId);
                        cachedUsers.Add(message.UserId, user);
                    }

                    if (user is null) sb.AppendLine($"Unknown member ({user.Id})");
                    else sb.AppendLine($"{user} ({user.Id})");
                    sb.AppendLine($" at {Helper.ToUniversalDateTime(message.Timestamp)} UTC");

                    string messageContent = "    " + message.Content.Value.Replace("\n", "\n    ");

                    sb.AppendLine($"{messageContent}\n");
                }

                string content = sb.ToString().TrimEnd('\r', '\n');
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
