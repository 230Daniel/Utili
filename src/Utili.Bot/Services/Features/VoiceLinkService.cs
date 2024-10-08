﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Utili.Database.Entities;
using Utili.Database.Extensions;
using Qommon;
using Utili.Bot.Extensions;

namespace Utili.Bot.Services;

public class VoiceLinkService
{
    private readonly ILogger<VoiceLinkService> _logger;
    private readonly UtiliDiscordBot _bot;
    private readonly IServiceScopeFactory _scopeFactory;

    private List<(ulong, ulong)> _channelsRequiringUpdate;

    public VoiceLinkService(ILogger<VoiceLinkService> logger, UtiliDiscordBot bot, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _bot = bot;
        _scopeFactory = scopeFactory;

        _channelsRequiringUpdate = new List<(ulong, ulong)>();
    }

    public async Task VoiceStateUpdated(IServiceScope scope, VoiceStateUpdatedEventArgs e)
    {
        try
        {
            var db = scope.GetDbContext();
            var config = await db.VoiceLinkConfigurations.GetForGuildAsync(e.GuildId);
            if (config is null || !config.Enabled) return;

            lock (_channelsRequiringUpdate)
            {
                if (e.NewVoiceState.ChannelId is not null &&
                    !config.ExcludedChannels.Contains(e.NewVoiceState.ChannelId.Value))
                    _channelsRequiringUpdate.Add((e.GuildId, e.NewVoiceState.ChannelId.Value));
                if (e.OldVoiceState?.ChannelId is not null &&
                    !config.ExcludedChannels.Contains(e.OldVoiceState.ChannelId.Value))
                    _channelsRequiringUpdate.Add((e.GuildId, e.OldVoiceState.ChannelId.Value));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown on voice state updated");
        }
    }

    public void Start()
    {
        _ = UpdateLinkedChannelsAsync();
    }

    private async Task UpdateLinkedChannelsAsync()
    {
        while (true)
        {
            try
            {
                List<(ulong, ulong)> channelsToUpdate;

                lock (_channelsRequiringUpdate)
                {
                    channelsToUpdate = _channelsRequiringUpdate.Distinct().ToList();
                    _channelsRequiringUpdate.Clear();
                }

                var tasks = channelsToUpdate.Select(x => UpdateLinkedChannelWithTimeoutAsync(x.Item1, x.Item2)).ToList();
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception thrown starting channel updates");
            }

            await Task.Delay(1500);
        }
    }

    private async Task UpdateLinkedChannelWithTimeoutAsync(ulong guildId, ulong channelId)
    {
        var tcs = new CancellationTokenSource();
        var task = UpdateLinkedChannelAsync(guildId, channelId, tcs.Token);
        var timeout = Task.Delay(5000, tcs.Token);

        if (await Task.WhenAny(task, timeout) == task)
        {
            await task;
        }
        else
        {
            _logger.LogWarning("Update for channel {GuildId}/{ChannelId} timed out", guildId, channelId);
            tcs.Cancel();
        }
    }

    private async Task UpdateLinkedChannelAsync(ulong guildId, ulong channelId, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.GetDbContext();

            var config = await db.VoiceLinkConfigurations.GetForGuildAsync(guildId);
            if (config is null || !config.Enabled || config.ExcludedChannels.Contains(channelId)) return;

            var channelRecord = await db.VoiceLinkChannels.GetForGuildChannelAsync(guildId, channelId);

            var guild = _bot.GetGuild(guildId);
            var voiceChannel = guild.GetAudioChannel(channelId);
            if (voiceChannel is null)
            {
                await CloseLinkedChannelAsync(scope, guild, config, channelRecord, cancellationToken);
                return;
            }

            var category = voiceChannel.CategoryId.HasValue ? guild.GetCategoryChannel(voiceChannel.CategoryId.Value) : null;

            if (!voiceChannel.BotHasPermissions(Permissions.ViewChannels)) return;
            if (category is not null && !category.BotHasPermissions(Permissions.ViewChannels | Permissions.ManageChannels | Permissions.ManageRoles)) return;
            if (category is null && !guild.BotHasPermissions(Permissions.ViewChannels | Permissions.ManageChannels | Permissions.ManageRoles)) return;

            var voiceStates = guild.GetVoiceStates().Where(x => x.Value.ChannelId == voiceChannel.Id).Select(x => x.Value).ToList();
            var connectedUsers = guild.Members.Values.Where(x => voiceStates.Any(y => y.MemberId == x.Id)).ToList();

            if (connectedUsers.All(x => x.IsBot))
            {
                await CloseLinkedChannelAsync(scope, guild, config, channelRecord, cancellationToken);
                return;
            }

            ITextChannel textChannel = channelRecord is not null
                ? guild.GetTextChannel(channelRecord.TextChannelId)
                : null;
            if (textChannel is null)
            {
                textChannel = await guild.CreateTextChannelAsync($"{config.ChannelPrefix}{voiceChannel.Name}", x =>
                {
                    if (voiceChannel.CategoryId.HasValue) x.CategoryId = voiceChannel.CategoryId.Value;
                    x.Topic = $"Users in {voiceChannel.Name} have access - Created by Utili";
                    x.Overwrites = new List<LocalOverwrite>
                    {
                        LocalOverwrite.Member(_bot.CurrentUser.Id, new OverwritePermissions().Allow(Permissions.ViewChannels)),
                        LocalOverwrite.Role(guildId, new OverwritePermissions().Deny(Permissions.ViewChannels)) // @everyone
                    };
                }, new DefaultRestRequestOptions { Reason = "Voice Link" }, cancellationToken);

                if (channelRecord is null)
                {
                    channelRecord = new VoiceLinkChannel(guildId, channelId)
                    {
                        TextChannelId = textChannel.Id
                    };
                    db.VoiceLinkChannels.Add(channelRecord);
                }
                else
                {
                    channelRecord.TextChannelId = textChannel.Id;
                    db.VoiceLinkChannels.Update(channelRecord);
                }

                await db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                if (!textChannel.BotHasPermissions(Permissions.ViewChannels | Permissions.ManageChannels | Permissions.ManageRoles)) return;
            }

            var overwrites = textChannel.Overwrites.Select(x => new LocalOverwrite(x.TargetId, x.TargetType, x.Permissions)).ToList();
            var overwritesChanged = false;

            overwrites.RemoveAll(x =>
            {
                if (x.TargetType == OverwriteTargetType.Member && x.TargetId != _bot.CurrentUser.Id)
                {
                    var member = guild.GetMember(x.TargetId.Value);
                    if (member is null || voiceStates.All(y => y.MemberId != member.Id) || voiceStates.First(y => y.MemberId == member.Id).ChannelId == voiceChannel.Id)
                    {
                        overwritesChanged = true;
                        return true;
                    }
                }
                return false;
            });

            foreach (var member in connectedUsers)
            {
                if (!overwrites.Any(x => x.TargetId == member.Id && x.TargetType == OverwriteTargetType.Member))
                {
                    overwritesChanged = true;
                    overwrites.Add(LocalOverwrite.Member(member.Id, new OverwritePermissions().Allow(Permissions.ViewChannels)));
                }
            }

            var everyoneOverwrite = overwrites.FirstOrDefault(x => x.TargetId == guildId && x.TargetType == OverwriteTargetType.Role);
            if (everyoneOverwrite is null || everyoneOverwrite.Permissions.Value.Denied.HasFlag(Permissions.ViewChannels))
            {
                overwritesChanged = true;
                overwrites.Remove(everyoneOverwrite);
                overwrites.Add(new LocalOverwrite(guildId, OverwriteTargetType.Role, new OverwritePermissions().Deny(Permissions.ViewChannels)));
            }

            if (overwritesChanged)
            {
                await textChannel.ModifyAsync(x => x.Overwrites = new Optional<IEnumerable<LocalOverwrite>>(overwrites), new DefaultRestRequestOptions { Reason = "Voice Link" }, cancellationToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception thrown updating linked channel {GuildId}/{ChannelId}", guildId, channelId);
        }
    }

    private async Task CloseLinkedChannelAsync(IServiceScope scope, IGuild guild, VoiceLinkConfiguration config, VoiceLinkChannel channelRecord, CancellationToken cancellationToken)
    {
        if (channelRecord is null) return;
        var textChannel = guild.GetTextChannel(channelRecord.TextChannelId);
        if (textChannel is null || !textChannel.BotHasPermissions(Permissions.ViewChannels | Permissions.ManageChannels)) return;

        if (config.DeleteChannels)
        {
            await textChannel.DeleteAsync(new DefaultRestRequestOptions { Reason = "Voice Link" }, cancellationToken);
            channelRecord.TextChannelId = 0;

            var db = scope.GetDbContext();
            db.VoiceLinkChannels.Remove(channelRecord);
            await db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            // Remove all permission overwrites except @everyone and utili
            var overwrites = textChannel.Overwrites.Select(x => new LocalOverwrite(x.TargetId, x.TargetType, x.Permissions)).ToList();
            overwrites.RemoveAll(x => x.TargetId != guild.Id && x.TargetId != _bot.CurrentUser.Id);
            await textChannel.ModifyAsync(x => x.Overwrites = new Optional<IEnumerable<LocalOverwrite>>(overwrites), new DefaultRestRequestOptions { Reason = "Voice Link" }, cancellationToken);
        }
    }
}
