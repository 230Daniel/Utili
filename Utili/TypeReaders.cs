using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using static Utili.Program;

namespace Utili
{
    public class UserTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            SocketGuild guild = context.Guild as SocketGuild;

            if (MentionUtils.TryParseUser(input, out ulong userId))
            {
                IUser user = guild.GetUser(userId);
                if (user is not null) return TypeReaderResult.FromSuccess(user);

                user = await _oldRest.GetGuildUserAsync(guild.Id, userId);
                if (user is not null) return TypeReaderResult.FromSuccess(user);
            }

            IReadOnlyCollection<IUser> users = await context.Guild.SearchUsersAsync(input, 1);
            return users.Count > 0 ? TypeReaderResult.FromSuccess(users.First()) : TypeReaderResult.FromError(CommandError.ParseFailed, "Input could not be parsed as a user");
        }
    }
}
