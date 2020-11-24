using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using static Utili.Program;

namespace Utili.Handlers
{
    internal class ReactionHandler
    {
        public static async Task ReactionAdded(Cacheable<IUserMessage, ulong> partialMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            _ = Task.Run(async () =>
            {
                IGuild guild = (channel as IGuildChannel).Guild;
                IUserMessage message = await partialMessage.GetOrDownloadAsync();
                IUser reactor = await _rest.GetGuildUserAsync(guild.Id, reaction.UserId);
                IEmote emote = reaction.Emote;
                

                await _reputation.ReactionAdded(guild, message, reactor, emote);
            });
        }

        public static async Task ReactionRemoved(Cacheable<IUserMessage, ulong> partialMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            _ = Task.Run(async () =>
            {
                IGuild guild = (channel as IGuildChannel).Guild;
                IUserMessage message = await partialMessage.GetOrDownloadAsync();
                IUser reactor = await _rest.GetGuildUserAsync(guild.Id, reaction.UserId);
                IEmote emote = reaction.Emote;

                await _reputation.ReactionRemoved(guild, message, reactor, emote);
            });
        }

        public static async Task ReactionsCleared(Cacheable<IUserMessage, ulong> partialMessage, ISocketMessageChannel channel)
        {
            
        }

        public static async Task ReactionsRemovedForEmote(Cacheable<IUserMessage, ulong> partialMessage, ISocketMessageChannel channel, IEmote emote)
        {
            
        }
    }
}
