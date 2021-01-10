using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Utili.Features;

namespace Utili.Handlers
{
    internal static class ReactionHandler
    {
        public static async Task ReactionAdded(Cacheable<IUserMessage, ulong> partialMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            _ = Task.Run(async () =>
            {
                IGuild guild = (channel as IGuildChannel).Guild;
                IEmote emote = reaction.Emote;
                
                await Reputation.ReactionAdded(guild, partialMessage, reaction.UserId, emote);
            });
        }

        public static async Task ReactionRemoved(Cacheable<IUserMessage, ulong> partialMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            _ = Task.Run(async () =>
            {
                IGuild guild = (channel as IGuildChannel).Guild;
                IEmote emote = reaction.Emote;

                await Reputation.ReactionRemoved(guild, partialMessage, reaction.UserId, emote);
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
