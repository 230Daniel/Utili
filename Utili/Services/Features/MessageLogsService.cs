using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NewDatabase.Entities;
using NewDatabase.Extensions;
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

        public async Task MessageReceived(IServiceScope scope, MessageReceivedEventArgs e)
        {
            try
            {
                if(e.Message.Author.IsBot || !e.GuildId.HasValue) return;

                var db = scope.GetDbContext();
                var config = await db.MessageLogsConfigurations.GetForGuildAsync(e.GuildId.Value);
                if (config is null || (config.DeletedChannelId == 0 && config.EditedChannelId == 0) || config.ExcludedChannels.Contains(e.ChannelId)) return;

                var message = new MessageLogsMessage(e.MessageId)
                {
                    GuildId = e.GuildId.Value,
                    ChannelId = e.ChannelId,
                    AuthorId = e.Message.Author.Id,
                    Timestamp = e.Message.CreatedAt().UtcDateTime,
                    Content = e.Message.Content
                };

                db.MessageLogsMessages.Add(message);

                if (!await db.GetIsGuildPremiumAsync(e.GuildId.Value))
                {
                    var messages = await db.MessageLogsMessages
                        .Where(x => x.GuildId == e.GuildId.Value && x.ChannelId == e.ChannelId)
                        .OrderByDescending(x => x.Timestamp)
                        .ToListAsync();

                    var excessMessages = messages.Skip(50);
                    if(excessMessages.Any()) db.MessageLogsMessages.RemoveRange(excessMessages);
                }
                
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message received ({Guild}/{Channel}/{Message})", e.GuildId, e.ChannelId, e.MessageId);
            }
        }

        public async Task MessageUpdated(IServiceScope scope, MessageUpdatedEventArgs e)
        {
            try
            {
                if (!e.GuildId.HasValue) return;

                ITextChannel channel = _client.GetTextChannel(e.GuildId.Value, e.ChannelId);

                var db = scope.GetDbContext();
                var config = await db.MessageLogsConfigurations.GetForGuildAsync(e.GuildId.Value);
                if (config is null || (config.DeletedChannelId == 0 && config.EditedChannelId == 0) || config.ExcludedChannels.Contains(e.ChannelId)) return;

                var messageRecord = await db.MessageLogsMessages.GetForMessageAsync(e.MessageId);
                if (messageRecord is null || !e.Model.Content.HasValue || e.Model.Content.Value == messageRecord.Content) return;

                var newMessage = e.NewMessage ?? await channel.FetchMessageAsync(e.MessageId) as IUserMessage;
                var embed = GetEditedEmbed(newMessage, messageRecord);

                messageRecord.Content = e.Model.Content.Value;
                db.MessageLogsMessages.Update(messageRecord);
                await db.SaveChangesAsync();

                ITextChannel logChannel = _client.GetTextChannel(e.GuildId.Value, config.EditedChannelId);
                if (logChannel is not null) await logChannel.SendEmbedAsync(embed);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message updated");
            }
        }

        public async Task MessageDeleted(IServiceScope scope, MessageDeletedEventArgs e)
        {
            try
            {
                if (!e.GuildId.HasValue) return;

                var db = scope.GetDbContext();
                var config = await db.MessageLogsConfigurations.GetForGuildAsync(e.GuildId.Value);
                if (config is null || (config.DeletedChannelId == 0 && config.EditedChannelId == 0) || config.ExcludedChannels.Contains(e.ChannelId)) return;

                var messageRecord = await db.MessageLogsMessages.GetForMessageAsync(e.MessageId);
                if (messageRecord is null) return;

                var member = _client.GetMember(e.GuildId.Value, messageRecord.AuthorId) ?? await _client.FetchMemberAsync(e.GuildId.Value, messageRecord.AuthorId);
                if (member is not null && member.IsBot) return;

                var embed = GetDeletedEmbed(messageRecord, member);

                db.MessageLogsMessages.Remove(messageRecord);
                await db.SaveChangesAsync();
                
                ITextChannel logChannel = _client.GetTextChannel(e.GuildId.Value, config.DeletedChannelId);
                if (logChannel is not null) await logChannel.SendEmbedAsync(embed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message deleted");
            }
        }

        public async Task MessagesDeleted(IServiceScope scope, MessagesDeletedEventArgs e)
        {
            try
            {
                var db = scope.GetDbContext();
                var config = await db.MessageLogsConfigurations.GetForGuildAsync(e.GuildId);
                if (config is null || (config.DeletedChannelId == 0 && config.EditedChannelId == 0) || config.ExcludedChannels.Contains(e.ChannelId)) return;

                var messages = await db.MessageLogsMessages.Where(x => e.MessageIds.Contains(x.MessageId)).ToListAsync();

                var embed = GetBulkDeletedEmbed(e, await PasteMessagesAsync(messages, e.MessageIds.Count), messages.Count);

                if (messages.Any())
                {
                    db.MessageLogsMessages.RemoveRange(messages);
                    await db.SaveChangesAsync();
                }
                
                ITextChannel logChannel = _client.GetTextChannel(e.GuildId, config.DeletedChannelId);
                if (logChannel is not null) await logChannel.SendEmbedAsync(embed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on messages deleted");
            }
        }

        private LocalEmbed GetEditedEmbed(IUserMessage newMessage, MessageLogsMessage messageRecord)
        {
            var builder = new LocalEmbed()
                .WithColor(new Color(66, 182, 245))
                .WithDescription($"**Message by {newMessage.Author.Mention} edited in {Mention.TextChannel(newMessage.ChannelId)}** [Jump]({newMessage.GetJumpUrl(messageRecord.GuildId)})")
                .WithAuthor(newMessage.Author)
                .WithFooter($"Message {messageRecord.MessageId}")
                .WithTimestamp(DateTime.SpecifyKind(messageRecord.Timestamp, DateTimeKind.Utc));

            if (messageRecord.Content.Length > 1024 || newMessage.Content.Length > 1024)
            {
                if (messageRecord.Content.Length < 2024 - builder.Description.Length - 2)
                    builder.Description += $"\n{messageRecord.Content}";
                else
                    builder.Description += "\nThe message is too large to fit in this embed";
            }
            else
            {
                builder.AddField("Before", messageRecord.Content);
                builder.AddField("After", newMessage.Content);
            }

            return builder;
        }

        private LocalEmbed GetDeletedEmbed(MessageLogsMessage deletedMessage, IMember member)
        {
            var builder = new LocalEmbed()
                .WithColor(new Color(245, 66, 66))
                .WithDescription($"**Message by {Mention.User(deletedMessage.AuthorId)} deleted in {Mention.TextChannel(deletedMessage.ChannelId)}**")
                .WithFooter($"Message {deletedMessage.MessageId}")
                .WithTimestamp(DateTime.SpecifyKind(deletedMessage.Timestamp, DateTimeKind.Utc));

            if (member is null) builder.WithAuthor("Unknown member");
            else builder.WithAuthor(member);

            if (deletedMessage.Content.Length > 2024 - builder.Description.Length - 2)
                builder.Description += "\nThe message is too large to fit in this embed";
            else
                builder.Description += $"\n{deletedMessage.Content}";

            return builder;
        }

        private LocalEmbed GetBulkDeletedEmbed(MessagesDeletedEventArgs e, string paste, int loggedCount)
        {
            var builder = new LocalEmbed()
                .WithColor(new Color(245, 66, 66))
                .WithDescription($"**{e.MessageIds.Count} messages bulk deleted in {Mention.TextChannel(e.ChannelId)}**\n" +
                                 $"[View {loggedCount} logged message{(loggedCount == 1 ? "" : "s")}]({paste})")
                .WithAuthor("Bulk Deletion");

            return builder;
        }

        private async Task<string> PasteMessagesAsync(List<MessageLogsMessage> messages, int total)
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
                    if (!cachedUsers.TryGetValue(message.AuthorId, out var user))
                    {
                        user = await _client.FetchUserAsync(message.AuthorId);
                        cachedUsers.Add(message.AuthorId, user);
                        await Task.Delay(500);
                    }

                    if (user is null) sb.AppendLine($"Unknown member ({user.Id})");
                    else sb.AppendLine($"{user} ({user.Id})");
                    sb.AppendLine($" at {message.Timestamp.ToUniversalFormat()} UTC");

                    var messageContent = "    " + message.Content.Replace("\n", "\n    ");

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
