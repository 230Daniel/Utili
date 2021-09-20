using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Database.Entities;
using Database.Extensions;
using Utili.Extensions;

namespace Utili.Services
{
    public class VoiceLinkService
    {
        private readonly ILogger<VoiceLinkService> _logger;
        private readonly DiscordClientBase _client;
        private readonly IServiceScopeFactory _scopeFactory;
        
        private List<(ulong, ulong)> _channelsRequiringUpdate;
        
        public VoiceLinkService(ILogger<VoiceLinkService> logger, DiscordClientBase client, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _client = client;
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
                    if (e.NewVoiceState?.ChannelId is not null &&
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

                    var tasks = channelsToUpdate.Select(x => UpdateLinkedChannelAsync(x.Item1, x.Item2)).ToList();
                    await Task.WhenAll(tasks);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception thrown starting channel updates");
                }

                await Task.Delay(250);
            }
        }

        private Task UpdateLinkedChannelAsync(ulong guildId, ulong channelId)
        {
            return Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.GetDbContext();
                    
                    var config = await db.VoiceLinkConfigurations.GetForGuildAsync(guildId);
                    if(config is null || !config.Enabled || config.ExcludedChannels.Contains(channelId)) return;
                    
                    var channelRecord = await db.VoiceLinkChannels.GetForGuildChannelAsync(guildId, channelId);
                    
                    var guild = _client.GetGuild(guildId);
                    var voiceChannel = guild.GetAudioChannel(channelId);
                    if (voiceChannel is null)
                    {
                        await CloseLinkedChannelAsync(scope, guild, config, channelRecord);
                        return;
                    }
                    
                    var category = voiceChannel.CategoryId.HasValue ? guild.GetCategoryChannel(voiceChannel.CategoryId.Value) : null;

                    if(!voiceChannel.BotHasPermissions(Permission.ViewChannels)) return;
                    if (category is not null && !category.BotHasPermissions(Permission.ViewChannels | Permission.ManageChannels | Permission.ManageRoles)) return;
                    if (category is null && !guild.BotHasPermissions(Permission.ViewChannels | Permission.ManageChannels | Permission.ManageRoles)) return;

                    var voiceStates = guild.GetVoiceStates().Where(x => x.Value.ChannelId == voiceChannel.Id).Select(x => x.Value).ToList();
                    var connectedUsers = guild.Members.Values.Where(x => voiceStates.Any(y => y.MemberId == x.Id)).ToList();

                    if (connectedUsers.All(x => x.IsBot))
                    {
                        await CloseLinkedChannelAsync(scope, guild, config, channelRecord);
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
                                LocalOverwrite.Member(_client.CurrentUser.Id, new OverwritePermissions().Allow(Permission.ViewChannels)),
                                LocalOverwrite.Role(guildId, new OverwritePermissions().Deny(Permission.ViewChannels)) // @everyone
                            };
                        }, new DefaultRestRequestOptions{Reason = "Voice Link"});

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
                        
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        if(!textChannel.BotHasPermissions(Permission.ViewChannels | Permission.ManageChannels | Permission.ManageRoles)) return;
                    }

                    var overwrites = textChannel.Overwrites.Select(x => new LocalOverwrite(x.TargetId, x.TargetType, x.Permissions)).ToList();
                    var overwritesChanged = false;

                    overwrites.RemoveAll(x =>
                    {
                        if (x.TargetType == OverwriteTargetType.Member && x.TargetId != _client.CurrentUser.Id)
                        {
                            IMember member = guild.GetMember(x.TargetId);
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
                            overwrites.Add(LocalOverwrite.Member(member.Id, new OverwritePermissions().Allow(Permission.ViewChannels)));
                        }
                    }

                    var everyoneOverwrite = overwrites.FirstOrDefault(x => x.TargetId == guildId && x.TargetType == OverwriteTargetType.Role);
                    if (everyoneOverwrite is null || everyoneOverwrite.Permissions.Denied.ViewChannels)
                    {
                        overwritesChanged = true;
                        overwrites.Remove(everyoneOverwrite);
                        overwrites.Add(new LocalOverwrite(guildId, OverwriteTargetType.Role, new OverwritePermissions().Deny(Permission.ViewChannels)));
                    }

                    if (overwritesChanged)
                    {
                        await textChannel.ModifyAsync(x => x.Overwrites = new Optional<IEnumerable<LocalOverwrite>>(overwrites), new DefaultRestRequestOptions{Reason = "Voice Link"});
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Exception thrown updating linked channel {guildId}/{channelId}");
                }
            });
        }

        private async Task CloseLinkedChannelAsync(IServiceScope scope, IGuild guild, VoiceLinkConfiguration config, VoiceLinkChannel channelRecord)
        {
            if (channelRecord is null) return;
            var textChannel = guild.GetTextChannel(channelRecord.TextChannelId);
            if (textChannel is null || !textChannel.BotHasPermissions(Permission.ViewChannels | Permission.ManageChannels)) return;
            
            if (config.DeleteChannels)
            {
                await textChannel.DeleteAsync(new DefaultRestRequestOptions{Reason = "Voice Link"});
                channelRecord.TextChannelId = 0;
                
                var db = scope.GetDbContext();
                db.VoiceLinkChannels.Remove(channelRecord);
                await db.SaveChangesAsync();
            }
            else
            {
                // Remove all permission overwrites except @everyone and utili
                var overwrites = textChannel.Overwrites.Select(x => new LocalOverwrite(x.TargetId, x.TargetType, x.Permissions)).ToList();
                overwrites.RemoveAll(x => x.TargetId != guild.Id && x.TargetId != _client.CurrentUser.Id);
                await textChannel.ModifyAsync(x => x.Overwrites = new Optional<IEnumerable<LocalOverwrite>>(overwrites), new DefaultRestRequestOptions {Reason = "Voice Link"});
            }
        }
    }
}
