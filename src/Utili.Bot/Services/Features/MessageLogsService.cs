using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Utili.Database.Entities;
using Utili.Database.Extensions;
using Utili.Bot.Extensions;

namespace Utili.Bot.Services;

public class MessageLogsService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MessageLogsService> _logger;
    private readonly UtiliDiscordBot _bot;
    private readonly HasteService _haste;
    private readonly IsPremiumService _isPremiumService;

    private readonly Timer _timer;

    public MessageLogsService(
        IServiceScopeFactory scopeFactory,
        ILogger<MessageLogsService> logger,
        UtiliDiscordBot bot,
        HasteService haste,
        IsPremiumService isPremiumService)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _bot = bot;
        _haste = haste;
        _isPremiumService = isPremiumService;

        _timer = new Timer(60000);
        _timer.Elapsed += (_, _) => _ = Delete30DayMessagesAsync();
    }

    public void Start()
    {
        _timer.Start();
    }

    public async Task MessageReceived(IServiceScope scope, MessageReceivedEventArgs e)
    {
        try
        {
            if (e.Message.Author.IsBot) return;

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

            if (e.Channel is IThreadChannel threadChannel)
            {
                if (!config.LogThreads || config.ExcludedChannels.Contains(threadChannel.ChannelId)) return;
                message.TextChannelId = threadChannel.ChannelId;
            }

            db.MessageLogsMessages.Add(message);

            if (!await _isPremiumService.GetIsGuildPremiumAsync(e.GuildId.Value))
            {
                var messages = await db.MessageLogsMessages
                    .Where(x => x.GuildId == e.GuildId.Value.RawValue && x.ChannelId == message.ChannelId)
                    .OrderByDescending(x => x.Timestamp)
                    .ToListAsync();

                var excessMessages = messages.Skip(e.Channel is IThreadChannel ? 15 : 30);
                if (excessMessages.Any()) db.MessageLogsMessages.RemoveRange(excessMessages);
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

            var db = scope.GetDbContext();
            var config = await db.MessageLogsConfigurations.GetForGuildAsync(e.GuildId.Value);
            if (config is null || (config.DeletedChannelId == 0 && config.EditedChannelId == 0) || config.ExcludedChannels.Contains(e.ChannelId)) return;

            IMessageGuildChannel channel = _bot.GetMessageGuildChannel(e.GuildId.Value, e.ChannelId);
            if (channel is IThreadChannel threadChannel && (!config.LogThreads || config.ExcludedChannels.Contains(threadChannel.ChannelId)))
                return;

            var messageRecord = await db.MessageLogsMessages.GetForMessageAsync(e.MessageId);
            if (messageRecord is null || !e.Model.Content.HasValue || e.Model.Content.Value == messageRecord.Content) return;

            var newMessage = e.NewMessage ?? await channel.FetchMessageAsync(e.MessageId) as IUserMessage;
            var embed = GetEditedEmbed(newMessage, messageRecord);

            messageRecord.Content = e.Model.Content.Value;
            db.MessageLogsMessages.Update(messageRecord);
            await db.SaveChangesAsync();

            var logChannel = _bot.GetTextChannel(e.GuildId.Value, config.EditedChannelId);
            if (logChannel is not null) await logChannel.SendEmbedAsync(embed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown on message updated");
        }
    }

    public async Task MessageDeleted(IServiceScope scope, MessageDeletedEventArgs e)
    {
        try
        {
            var db = scope.GetDbContext();
            var config = await db.MessageLogsConfigurations.GetForGuildAsync(e.GuildId.Value);
            if (config is null || (config.DeletedChannelId == 0 && config.EditedChannelId == 0) || config.ExcludedChannels.Contains(e.ChannelId)) return;

            IMessageGuildChannel channel = _bot.GetMessageGuildChannel(e.GuildId.Value, e.ChannelId);
            if (channel is IThreadChannel threadChannel && (!config.LogThreads || config.ExcludedChannels.Contains(threadChannel.ChannelId)))
                return;

            var messageRecord = await db.MessageLogsMessages.GetForMessageAsync(e.MessageId);
            if (messageRecord is null) return;

            var member = _bot.GetMember(e.GuildId.Value, messageRecord.AuthorId) ?? await _bot.FetchMemberAsync(e.GuildId.Value, messageRecord.AuthorId);
            if (member is not null && member.IsBot) return;

            var embed = GetDeletedEmbed(messageRecord, member);

            db.MessageLogsMessages.Remove(messageRecord);
            await db.SaveChangesAsync();

            var logChannel = _bot.GetTextChannel(e.GuildId.Value, config.DeletedChannelId);
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

            IMessageGuildChannel channel = _bot.GetMessageGuildChannel(e.GuildId, e.ChannelId);
            if (channel is IThreadChannel threadChannel && (!config.LogThreads || config.ExcludedChannels.Contains(threadChannel.ChannelId)))
                return;

            var messageIds = e.MessageIds.Select(x => x.RawValue);
            var messages = await db.MessageLogsMessages.Where(x => messageIds.Contains(x.MessageId)).ToListAsync();

            var embed = await GetBulkDeletedEmbedAsync(messages, e.MessageIds.Count, channel);

            if (messages.Any())
            {
                db.MessageLogsMessages.RemoveRange(messages);
                await db.SaveChangesAsync();
            }

            var logChannel = _bot.GetTextChannel(e.GuildId, config.DeletedChannelId);
            if (logChannel is not null) await logChannel.SendEmbedAsync(embed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown on messages deleted");
        }
    }

    private LocalEmbed GetEditedEmbed(IUserMessage newMessage, MessageLogsMessage messageRecord)
    {
        var embed = new LocalEmbed()
            .WithColor(new Color(66, 182, 245))
            .WithDescription($"**Message by {newMessage.Author.Mention} edited in {Mention.Channel(messageRecord.ChannelId)}** [Jump]({newMessage.GetJumpUrl(messageRecord.GuildId)})")
            .WithAuthor(newMessage.Author)
            .WithFooter($"Message {messageRecord.MessageId}")
            .WithTimestamp(DateTime.SpecifyKind(messageRecord.Timestamp, DateTimeKind.Utc));

        if (messageRecord.Content.Length > 1024 || newMessage.Content.Length > 1024)
        {
            if (messageRecord.Content.Length < 2024 - embed.Description.Value.Length - 2)
                embed.Description += $"\n{messageRecord.Content}";
            else
                embed.Description += "\nThe message is too large to fit in this embed";
        }
        else
        {
            embed.AddField("Before", messageRecord.Content);
            embed.AddField("After", newMessage.Content);
        }

        return embed;
    }

    private LocalEmbed GetDeletedEmbed(MessageLogsMessage messageRecord, IMember member)
    {
        var embed = new LocalEmbed()
            .WithColor(new Color(245, 66, 66))
            .WithDescription($"**Message by {Mention.User(messageRecord.AuthorId)} deleted in {Mention.Channel(messageRecord.ChannelId)}**")
            .WithFooter($"Message {messageRecord.MessageId}")
            .WithTimestamp(DateTime.SpecifyKind(messageRecord.Timestamp, DateTimeKind.Utc));

        if (member is null) embed.WithAuthor("Unknown member");
        else embed.WithAuthor(member);

        if (messageRecord.Content.Length > 2024 - embed.Description.Value.Length - 2)
            embed.Description += "\nThe message is too large to fit in this embed";
        else
            embed.Description += $"\n{messageRecord.Content}";

        return embed;
    }

    private async Task<LocalEmbed> GetBulkDeletedEmbedAsync(List<MessageLogsMessage> messageRecords, int count, IMessageGuildChannel channel)
    {
        if (messageRecords.Count == 0)
        {
            return new LocalEmbed()
                .WithColor(new Color(245, 66, 66))
                .WithDescription($"**{count} messages bulk deleted in {channel.Mention}**\n" +
                                 "No messages were logged")
                .WithAuthor("Bulk Deletion");
        }

        var paste = await PasteMessagesAsync(messageRecords, count);

        var link = paste is not null
            ? $"[View {messageRecords.Count} logged message{(messageRecords.Count == 1 ? "" : "s")}]({paste})"
            : "Exception thrown uploading messages to Haste server";

        return new LocalEmbed()
            .WithColor(new Color(245, 66, 66))
            .WithDescription($"**{count} messages bulk deleted in {Mention.Channel(messageRecords[0].ChannelId)}**\n" +
                             link)
            .WithAuthor("Bulk Deletion");
    }

    private async Task<string> PasteMessagesAsync(List<MessageLogsMessage> messages, int count)
    {
        try
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Logged {messages.Count} of {count} deleted messages");
            sb.AppendLine();
            sb.AppendLine();

            var cachedUsers = new Dictionary<Snowflake, IUser>();

            foreach (var message in messages)
            {
                if (!cachedUsers.TryGetValue(message.AuthorId, out var user))
                {
                    user = _bot.GetUser(message.AuthorId) as IUser ?? await _bot.FetchUserAsync(message.AuthorId);
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
            return null;
        }
    }

    private async Task Delete30DayMessagesAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.GetDbContext();

            var minTimestamp = DateTime.UtcNow - TimeSpan.FromDays(30);
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM message_logs_messages WHERE timestamp < {minTimestamp};");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown deleting 30 day messages");
        }
    }
}
