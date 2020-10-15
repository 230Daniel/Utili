using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Database.Data;
using Discord.Commands;
using Discord.WebSocket;
using static Utili.Program;
using static Utili.MessageSender;

namespace Utili.Handlers
{
    internal class MessageReceivedHandler
    {
        public static async Task MessageReceived(SocketMessage partialMessage)
        {
            _ = Task.Run(async () =>
            {
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

                        // TODO: LOG COMMANDS

                        if (!result.IsSuccess)
                        {
                            string errorReason = GetCommandErrorReason(result);

                            if (!string.IsNullOrEmpty(errorReason))
                            {
                                errorReason += "\n\nSupport Server";
                                await SendFailureAsync(context.Channel, "Command error", errorReason);
                            }
                        }
                    }
                }
            });
        }

        public static string GetCommandErrorReason(IResult result)
        {
            switch (result.Error)
            {
                case CommandError.BadArgCount:
                    return "Invalid amount of command arguments\nTry wrapping arguments with speech marks (\"...\")";

                case CommandError.ObjectNotFound:
                    return "Failed to interpret a command argument\nObject not found\nTry wrapping arguments with speech marks (\"...\")";

                case CommandError.MultipleMatches:
                    return "Failed to interpret a command argument\nMultiple matches\nTry wrapping arguments with speech marks (\"...\")";

                case CommandError.UnmetPrecondition:
                    return "Invalid command preconditions";

                case CommandError.Exception:
                    return "An error occured during command execution";

                default:
                    return null;
            }
        }
    }
}
