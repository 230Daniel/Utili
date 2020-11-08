using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Utili
{
    internal class UserTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)

        // Order of priority:
        // 1 - User ID from cache
        // 2 - User ID from discord
        // 3 - Username/Nickname

        {
            await Task.Yield();

            SocketGuild socketGuild = context.Guild as SocketGuild;

            if (ulong.TryParse(input, out ulong userId))
            {
                IGuildUser user = socketGuild.GetUser(userId);
                if (user != null) return TypeReaderResult.FromSuccess(user);

                user = await context.Guild.GetUserAsync(userId);
                if (user != null) return TypeReaderResult.FromSuccess(user);
            }

            //string lowerInput = input.ToLower();

            //IEnumerable<SocketGuildUser> cachedUsers = socketGuild.Users.Where(x =>  
            //    x.ToString().ToLower() == lowerInput || 
            //    x.Username.ToLower() == lowerInput ||
            //    x.Nickname.ToLower() == lowerInput);

            IReadOnlyCollection<IGuildUser> users = await context.Guild.SearchUsersAsync(input, 1);
            if (users.Count > 0) return TypeReaderResult.FromSuccess(users.First());

            return TypeReaderResult.FromError(CommandError.ParseFailed, "Input could not be parsed as a user");
        }
    }
}
