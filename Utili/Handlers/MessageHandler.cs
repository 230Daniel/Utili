using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using static Utili.Program;
using static Utili.MessageSender;

namespace Utili.Handlers
{
    internal class MessageHandler
    {
        public static async Task MessageReceived(SocketMessage partialMessage)
        {
            _ = Task.Run(async () =>
            {
                if (partialMessage.Author.Id == _client.CurrentUser.Id && partialMessage.GetType() == typeof(SocketSystemMessage))
                {
                    await partialMessage.DeleteAsync();
                    return;
                }

                SocketUserMessage message = partialMessage as SocketUserMessage;
                SocketTextChannel channel = message.Channel as SocketTextChannel;
                SocketGuild guild = channel.Guild;

                SocketCommandContext context = new SocketCommandContext(_client.GetShardFor(guild), message);

                if (!context.User.IsBot && !string.IsNullOrEmpty(context.Message.Content))
                {
                    string prefix = Misc.GetPrefix(guild.Id);

                    int argPos = 0;
                    if (context.Message.HasStringPrefix(prefix, ref argPos) ||
                        context.Message.HasMentionPrefix(_client.CurrentUser, ref argPos))
                    {
                        IResult result = await _commands.ExecuteAsync(context, argPos, null);

                        if (_config.LogCommands)
                        {
                            _logger.Log("Command", $"{context.Guild.Id} {context.User}: {context.Message.Content}");
                        }

                        if (!result.IsSuccess)
                        {
                            string errorReason = GetCommandErrorReason(result);

                            if (!string.IsNullOrEmpty(errorReason))
                            {
                                await SendFailureAsync(context.Channel, "Error", errorReason);
                            }
                        }
                    }
                }

                // High priority
                try { await _messageLogs.MessageReceived(context); } catch {}
                try { await _messageFilter.MessageReceived(context); } catch {}

                // Low priority
                _ = _voteChannels.MessageReceived(context);
                _ = _inactiveRole.UpdateUserAsync(context.Guild, context.User as SocketGuildUser);
                _ = _channelMirroring.MessageReceived(context);
                _ = _notices.MessageReceived(context, partialMessage);

            });
        }

        public static async Task MessageEdited(Cacheable<IMessage, ulong> partialMessage, SocketMessage message, ISocketMessageChannel channel)
        {
            _ = Task.Run(async () =>
            {
                if (message.Author.IsBot || channel is SocketDMChannel) return;

                SocketTextChannel guildChannel = channel as SocketTextChannel;

                SocketCommandContext context = new SocketCommandContext(Helper.GetShardForGuild(guildChannel.Guild), message as SocketUserMessage);

                _ = _messageLogs.MessageEdited(context);
            });
        }

        public static async Task MessageDeleted(Cacheable<IMessage, ulong> partialMessage, ISocketMessageChannel channel)
        {
            _ = Task.Run(async () =>
            {
                if (channel is SocketDMChannel) return;

                SocketTextChannel guildChannel = channel as SocketTextChannel;

                _ = _messageLogs.MessageDeleted(guildChannel.Guild, guildChannel, partialMessage.Id);
            });
        }

        public static async Task MessagesBulkDeleted(IReadOnlyCollection<Cacheable<IMessage, ulong>> messageIds, ISocketMessageChannel channel)
        {
            _ = Task.Run(async () =>
            {
                if (channel is SocketDMChannel) return;

                SocketTextChannel guildChannel = channel as SocketTextChannel;

                _ = _messageLogs.MessagesBulkDeleted(guildChannel.Guild, guildChannel,
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
