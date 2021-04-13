using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using Utili.Extensions;

namespace Utili.Services
{
    public class VoiceLinkService
    {
        ILogger<VoiceLinkService> _logger;
        DiscordClientBase _client;

        List<(ulong, ulong)> _channelsRequiringUpdate;

        public VoiceLinkService(ILogger<VoiceLinkService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;

            _channelsRequiringUpdate = new List<(ulong, ulong)>();
        }

        public async Task VoiceStateUpdated(object sender, VoiceStateUpdatedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                VoiceLinkRow row = await VoiceLink.GetRowAsync(e.GuildId);
                if (!row.Enabled) return;

                lock (_channelsRequiringUpdate)
                {
                    if (e.NewVoiceState?.ChannelId != null && !row.ExcludedChannels.Contains(e.NewVoiceState.ChannelId.Value))
                        _channelsRequiringUpdate.Add((e.GuildId, e.NewVoiceState.ChannelId.Value));
                    if (e.OldVoiceState?.ChannelId != null && !row.ExcludedChannels.Contains(e.OldVoiceState.ChannelId.Value))
                        _channelsRequiringUpdate.Add((e.GuildId, e.OldVoiceState.ChannelId.Value));
                }
            });
        }

        public void Start()
        {
            _ = UpdateLinkedChannelsAsync();
        }

        async Task UpdateLinkedChannelsAsync()
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

                    List<Task> tasks = channelsToUpdate.Select(x => UpdateLinkedChannelAsync(x.Item1, x.Item2)).ToList();
                    await Task.WhenAll(tasks);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception thrown starting channel updates");
                }

                await Task.Delay(250);
            }
        }

        Task UpdateLinkedChannelAsync(ulong guildId, ulong channelId)
        {
            return Task.Run(async () =>
            {
                try
                {
                    CachedGuild guild = _client.GetGuild(guildId);
                    CachedVoiceChannel voiceChannel = guild.GetVoiceChannel(channelId);
                    CachedCategoryChannel category = voiceChannel.CategoryId.HasValue ? guild.GetCategoryChannel(voiceChannel.CategoryId.Value) : null;

                    if(!voiceChannel.BotHasPermissions(_client, Permission.ViewChannel)) return;
                    if (category is not null && !category.BotHasPermissions(_client, Permission.ViewChannel, Permission.ManageChannels, Permission.ManageRoles)) return;
                    if (category is null && !guild.BotHasPermissions(_client, Permission.ViewChannel, Permission.ManageChannels, Permission.ManageRoles)) return;

                    List<IVoiceState> voiceStates = guild.VoiceStates.Values.Where(x => x.ChannelId == voiceChannel.Id).ToList();
                    List<IMember> connectedUsers = guild.Members.Values.Where(x => voiceStates.Any(y => y.MemberId == x.Id)).ToList();

                    VoiceLinkChannelRow channelRow = await VoiceLink.GetChannelRowAsync(guild.Id, voiceChannel.Id);
                    VoiceLinkRow metaRow = await VoiceLink.GetRowAsync(guild.Id);

                    ITextChannel textChannel = null;
                    try
                    {
                        textChannel = guild.GetTextChannel(channelRow.TextChannelId);
                    }
                    catch {}

                    if (connectedUsers.Count(x => !x.IsBot) == 0 && metaRow.DeleteChannels)
                    {
                        if(textChannel is null || !textChannel.BotHasPermissions(_client, Permission.ViewChannel, Permission.ManageChannels)) return;
                        await textChannel.DeleteAsync();
                        channelRow.TextChannelId = 0;
                        await VoiceLink.SaveChannelRowAsync(channelRow);
                        return;
                    }

                    List<LocalOverwrite> overwrites;

                    if (textChannel is null)
                    {
                        textChannel = await guild.CreateTextChannelAsync($"{metaRow.Prefix.Value}{voiceChannel.Name}", x =>
                        {
                            if (voiceChannel.CategoryId.HasValue) x.ParentId = voiceChannel.CategoryId.Value;
                            x.Topic = $"Users in {voiceChannel.Name} have access - Created by Utili";
                            x.Overwrites = new List<LocalOverwrite>
                            {
                                new LocalOverwrite(_client.CurrentUser, new OverwritePermissions().Allow(Permission.ViewChannel)),
                                new LocalOverwrite(guildId, OverwriteTargetType.Role, new OverwritePermissions().Deny(Permission.ViewChannel)) // @everyone
                            };
                        });

                        channelRow.TextChannelId = textChannel.Id;
                        await VoiceLink.SaveChannelRowAsync(channelRow);
                    }
                    else
                    {
                        if(!textChannel.BotHasPermissions(_client, Permission.ViewChannel, Permission.ManageChannels, Permission.ManageRoles)) return;
                    }

                    overwrites = textChannel.Overwrites.Select(x => new LocalOverwrite(x.TargetId, x.TargetType, x.Permissions)).ToList();
                    bool overwritesChanged = false;

                    overwrites.RemoveAll(x =>
                    {
                        if (x.TargetType == OverwriteTargetType.Member && x.TargetId != _client.CurrentUser.Id)
                        {
                            IMember member = guild.GetMember(x.TargetId);
                            if (voiceStates.All(y => y.MemberId != member.Id) || voiceStates.First(y => y.MemberId == member.Id).ChannelId == voiceChannel.Id)
                            {
                                overwritesChanged = true;
                                return true;
                            }
                        }
                        return false;
                    });

                    foreach (IMember member in connectedUsers)
                    {
                        if (!overwrites.Any(x => x.TargetId == member.Id && x.TargetType == OverwriteTargetType.Member))
                        {
                            overwritesChanged = true;
                            overwrites.Add(new LocalOverwrite(member, new OverwritePermissions().Allow(Permission.ViewChannel)));
                        }
                    }

                    LocalOverwrite everyoneOverwrite = overwrites.FirstOrDefault(x => x.TargetId == guildId && x.TargetType == OverwriteTargetType.Role);
                    if (everyoneOverwrite is null || everyoneOverwrite.Permissions.Denied.ViewChannel)
                    {
                        overwritesChanged = true;
                        overwrites.Remove(everyoneOverwrite);
                        overwrites.Add(new LocalOverwrite(guildId, OverwriteTargetType.Role, new OverwritePermissions().Deny(Permission.ViewChannel)));
                    }

                    if (overwritesChanged)
                    {
                        await textChannel.ModifyAsync(x => x.Overwrites = new Optional<IEnumerable<LocalOverwrite>>(overwrites));
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Exception thrown updating linked channel {guildId}/{channelId}");
                }
            });
        }
    }
}
