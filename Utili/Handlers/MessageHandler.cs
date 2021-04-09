using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using static Utili.Program;
using static Utili.MessageSender;
using ChannelMirroring = Utili.Features.ChannelMirroring;
using InactiveRole = Utili.Features.InactiveRole;
using MessageFilter = Utili.Features.MessageFilter;
using MessageLogs = Utili.Features.MessageLogs;
using Notices = Utili.Features.Notices;
using VoteChannels = Utili.Features.VoteChannels;

namespace Utili.Handlers
{
    internal static class MessageHandler
    {
        public static async Task MessageReceived(SocketMessage partialMessage)
        {
            _ = Task.Run(async () =>
            {
                if (partialMessage.Author.Id == _oldClient.CurrentUser.Id && partialMessage is SocketSystemMessage)
                {
                    await partialMessage.DeleteAsync();
                    return;
                }

                _ = Features.Autopurge.MessageReceived(partialMessage);

                SocketUserMessage message = partialMessage as SocketUserMessage;
                SocketTextChannel channel = message.Channel as SocketTextChannel;
                SocketGuild guild = channel.Guild;

                SocketCommandContext context = new SocketCommandContext(_oldClient.GetShardFor(guild), message);

                if (!context.User.IsBot && !string.IsNullOrEmpty(context.Message.Content))
                {
                    CoreRow row = await Core.GetRowAsync(context.Guild.Id);
                    bool excluded = row.ExcludedChannels.Contains(context.Channel.Id);
                    if (!row.EnableCommands) excluded = !excluded;

                    int argPos = 0;
                    if (!excluded && (context.Message.HasStringPrefix(row.Prefix.Value, ref argPos) ||
                        context.Message.HasMentionPrefix(_oldClient.CurrentUser, ref argPos)))
                    {
                        bool logCommand = _config.LogCommands;
                        IResult result = await _oldCommands.ExecuteAsync(context, argPos, null);

                        if (!result.IsSuccess)
                        {
                            string errorReason = GetCommandErrorReason(result);

                            if (!string.IsNullOrEmpty(errorReason)) await Context.Channel.SendFailureAsync("Error", errorReason);
                            else logCommand = false;
                        }

                        if (logCommand) _logger.Log("Command", context.Message.Content);
                    }
                }

                // High priority
                try { await MessageLogs.MessageReceived(context); } catch (Exception e) { _logger.ReportError("MsgReceived", e); }
                try { await MessageFilter.MessageReceived(context); } catch (Exception e) { _logger.ReportError("MsgReceived", e); }

                // Low priority
                _ = VoteChannels.MessageReceived(context);
                _ = InactiveRole.UpdateUserAsync(context.Guild, context.User as SocketGuildUser);
                _ = ChannelMirroring.MessageReceived(context);
                _ = Notices.MessageReceived(context);
            });
        }

        public static async Task MessageEdited(Cacheable<IMessage, ulong> partialMessage, SocketMessage message, ISocketMessageChannel channel)
        {
            _ = Task.Run(async () =>
            {
                if (channel is SocketDMChannel) return;

                _ = Features.Autopurge.MessageEdited(message);

                SocketTextChannel guildChannel = channel as SocketTextChannel;
                SocketCommandContext context = new SocketCommandContext(Helper.GetShardForGuild(guildChannel.Guild), message as SocketUserMessage);

                _ = MessageLogs.MessageEdited(context);
            });
        }

        public static async Task MessageDeleted(Cacheable<IMessage, ulong> partialMessage, ISocketMessageChannel channel)
        {
            _ = Task.Run(async () =>
            {
                if (channel is SocketDMChannel) return;

                SocketTextChannel guildChannel = channel as SocketTextChannel;

                _ = MessageLogs.MessageDeleted(guildChannel.Guild, guildChannel, partialMessage.Id);
            });
        }

        public static async Task MessagesBulkDeleted(IReadOnlyCollection<Cacheable<IMessage, ulong>> messageIds, ISocketMessageChannel channel)
        {
            _ = Task.Run(async () =>
            {
                if (channel is SocketDMChannel) return;

                SocketTextChannel guildChannel = channel as SocketTextChannel;

                _ = MessageLogs.MessagesBulkDeleted(guildChannel.Guild, guildChannel,
                    messageIds.Select(x => x.Id).ToList());
            });
        }

        public static string GetCommandErrorReason(IResult result)
        {
            return result.Error switch
            {
                CommandError.BadArgCount => "Invalid amount of command arguments",
                CommandError.ObjectNotFound => "Failed to interpret a command argument (Object not found)",
                CommandError.MultipleMatches => "Failed to interpret a command argument (Multiple matches)",
                CommandError.ParseFailed => "Failed to interpret a command argument (Parse failed)",
                CommandError.Exception => "An exception occured while trying to execute the command",
                CommandError.UnmetPrecondition => "Invalid command preconditions",
                CommandError.UnknownCommand => null,
                _ => "An error occured while trying to execute the command",
            };
        }
    }
}
