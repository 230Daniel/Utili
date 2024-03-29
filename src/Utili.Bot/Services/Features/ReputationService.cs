﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Utili.Database.Extensions;
using Utili.Bot.Extensions;

namespace Utili.Bot.Services;

public class ReputationService
{
    private readonly ILogger<ReputationService> _logger;
    private readonly UtiliDiscordBot _bot;

    public ReputationService(ILogger<ReputationService> logger, UtiliDiscordBot bot)
    {
        _logger = logger;
        _bot = bot;
    }

    public async Task ReactionAdded(IServiceScope scope, ReactionAddedEventArgs e)
    {
        try
        {
            if (!e.GuildId.HasValue) return;

            var guild = _bot.GetGuild(e.GuildId.Value);
            var channel = guild.GetMessageGuildChannel(e.ChannelId);

            var db = scope.GetDbContext();
            var config = await db.ReputationConfigurations.GetForGuildWithEmojisAsync(e.GuildId.Value);
            if (config is null) return;

            var emojiConfig = config.Emojis.FirstOrDefault(x => Equals(x.Emoji, e.Emoji.ToString()));
            if (emojiConfig is null) return;

            var message = e.Message ?? await channel.FetchMessageAsync(e.MessageId) as IUserMessage;
            if (message is null || message.Author.IsBot || message.Author.Id == e.UserId) return;

            var reactor = e.Member ?? await guild.FetchMemberAsync(e.UserId);
            if (reactor is null || reactor.IsBot) return;

            await db.ReputationMembers.UpdateMemberReputationAsync(e.GuildId.Value, message.Author.Id, emojiConfig.Value);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown on reaction added");
        }
    }

    public async Task ReactionRemoved(IServiceScope scope, ReactionRemovedEventArgs e)
    {
        try
        {
            if (!e.GuildId.HasValue) return;

            var guild = _bot.GetGuild(e.GuildId.Value);
            var channel = guild.GetMessageGuildChannel(e.ChannelId);

            var db = scope.GetDbContext();
            var config = await db.ReputationConfigurations.GetForGuildWithEmojisAsync(e.GuildId.Value);
            if (config is null) return;

            var emojiConfig = config.Emojis.FirstOrDefault(x => Equals(x.Emoji, e.Emoji.ToString()));
            if (emojiConfig is null) return;

            var message = e.Message ?? await channel.FetchMessageAsync(e.MessageId) as IUserMessage;
            if (message is null || message.Author.IsBot || message.Author.Id == e.UserId) return;

            var reactor = await guild.FetchMemberAsync(e.UserId);
            if (reactor is null || reactor.IsBot) return;

            await db.ReputationMembers.UpdateMemberReputationAsync(e.GuildId.Value, message.Author.Id, -emojiConfig.Value);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown on reaction removed");
        }
    }
}
