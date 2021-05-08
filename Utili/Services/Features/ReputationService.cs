using System;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using Utili.Extensions;

namespace Utili.Services
{
    public class ReputationService
    {
        ILogger<ReputationService> _logger;
        DiscordClientBase _client;

        public ReputationService(ILogger<ReputationService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task ReactionAdded(ReactionAddedEventArgs e)
        {
            try
            {
                if(!e.GuildId.HasValue) return;

                IGuild guild = _client.GetGuild(e.GuildId.Value);
                ITextChannel channel = guild.GetTextChannel(e.ChannelId);

                ReputationRow row = await Reputation.GetRowAsync(e.GuildId.Value);
                if (!row.Emotes.Any(x => guild.GetEmoji(x.Item1).Equals(e.Emoji))) return;
                int change = row.Emotes.First(x => Equals(x.Item1, e.Emoji.ToString())).Item2;

                IUserMessage message = e.Message ?? await channel.FetchMessageAsync(e.MessageId) as IUserMessage;
                if(message is null || message.Author.IsBot || message.Author.Id == e.UserId) return;

                IMember reactor = e.Member ?? await guild.FetchMemberAsync(e.UserId);
                if(reactor is null || reactor.IsBot) return;

                await Reputation.AlterUserReputationAsync(guild.Id, message.Author.Id, change);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on reaction added");
            }
        }

        public async Task ReactionRemoved(ReactionRemovedEventArgs e)
        {
            try
            {
                if(!e.GuildId.HasValue) return;

                IGuild guild = _client.GetGuild(e.GuildId.Value);
                ITextChannel channel = guild.GetTextChannel(e.ChannelId);

                ReputationRow row = await Reputation.GetRowAsync(e.GuildId.Value);
                if (!row.Emotes.Any(x => guild.GetEmoji(x.Item1).Equals(e.Emoji))) return;
                int change = -1 * row.Emotes.First(x => Equals(x.Item1, e.Emoji.ToString())).Item2;

                IUserMessage message = e.Message ?? await channel.FetchMessageAsync(e.MessageId) as IUserMessage;
                if(message is null || message.Author.IsBot || message.Author.Id == e.UserId) return;

                IMember reactor = await guild.FetchMemberAsync(e.UserId);
                if(reactor is null || reactor.IsBot) return;

                await Reputation.AlterUserReputationAsync(guild.Id, message.Author.Id, change);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on reaction removed");
            }
        }
    }
}
