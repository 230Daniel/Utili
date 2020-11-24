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
                IUserMessage message = await partialMessage.GetOrDownloadAsync();
                SocketGuildUser reactor = reaction.User.Value as SocketGuildUser; // TODO: Verify this works without user cache
                IEmote emote = reaction.Emote;

                await _reputation.ReactionAdded(message, reactor, emote);
            });
        }

        public static async Task ReactionRemoved(Cacheable<IUserMessage, ulong> partialMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            _ = Task.Run(async () =>
            {
                IUserMessage message = await partialMessage.GetOrDownloadAsync();
                SocketGuildUser reactor = reaction.User.Value as SocketGuildUser; // TODO: Verify this works without user cache
                IEmote emote = reaction.Emote;

                await _reputation.ReactionRemoved(message, reactor, emote);
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
