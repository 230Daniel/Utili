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

            if (ulong.TryParse(input.Replace("<", "").Replace("@", "").Replace("!", "").Replace(">", ""), out ulong userId))
            {
                IUser user = guild.GetUser(userId);
                if (user != null) return TypeReaderResult.FromSuccess(user);

                user = await _rest.GetGuildUserAsync(guild.Id, userId);
                if (user != null) return TypeReaderResult.FromSuccess(user);
            }

            IReadOnlyCollection<IUser> users = await context.Guild.SearchUsersAsync(input, 1);
            if (users.Count > 0) return TypeReaderResult.FromSuccess(users.First());

            return TypeReaderResult.FromError(CommandError.ParseFailed, "Input could not be parsed as a user");
        }
    }
}
