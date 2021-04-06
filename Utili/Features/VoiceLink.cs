using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database.Data;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using static Utili.Program;

namespace Utili.Features
{
    internal static class VoiceLink
    {
        private static Timer _timer;
        private static List<SocketVoiceChannel> _channelsRequiringUpdate = new List<SocketVoiceChannel>();
        private static bool _updating;

        public static void Start()
        {
            _timer?.Dispose();

            _timer = new Timer(2500);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        public static async Task RequestUpdateAsync(SocketVoiceChannel channel)
        {
            VoiceLinkRow metaRow = await Database.Data.VoiceLink.GetRowAsync(channel.Guild.Id);
            if (!metaRow.Enabled || metaRow.ExcludedChannels.Contains(channel.Id)) return;
            
            lock (_channelsRequiringUpdate)
            {
                _channelsRequiringUpdate.Add(channel);
            }
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateLinkedChannelsAsync().GetAwaiter().GetResult();
        }

        private static async Task UpdateLinkedChannelsAsync()
        {
            if(_updating) return;
            _updating = true;
            try
            {
                List<SocketVoiceChannel> channelsToUpdate = new List<SocketVoiceChannel>();

                lock (_channelsRequiringUpdate)
                {
                    foreach (SocketVoiceChannel voiceChannel in _channelsRequiringUpdate)
                        if (channelsToUpdate.All(x => x.Id != voiceChannel.Id))
                            channelsToUpdate.Add(voiceChannel);

                    _channelsRequiringUpdate.Clear();
                }

                List<Task> tasks = channelsToUpdate.Select(UpdateLinkedChannelAsync).ToList();
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                _logger.ReportError("VoiceLink", e);
            }
            _updating = false;
        }

        private static async Task UpdateLinkedChannelAsync(SocketVoiceChannel voiceChannel)
        {
            await Task.Delay(1);

            SocketGuild guild = voiceChannel.Guild;
            
            if(!voiceChannel.BotHasPermissions(ChannelPermission.ViewChannel)) return;
            if (voiceChannel.Category is not null)
                if(!voiceChannel.Category.BotHasPermissions(ChannelPermission.ViewChannel, ChannelPermission.ManageChannels, ChannelPermission.ManageRoles)) return;
            if(!guild.BotHasPermissions(GuildPermission.ViewChannel, GuildPermission.ManageChannels, GuildPermission.ManageRoles)) return;

            List<SocketGuildUser> connectedUsers =
                guild.Users.Where(x => x.VoiceChannel is not null && x.VoiceChannel.Id == voiceChannel.Id).ToList();

            VoiceLinkChannelRow channelRow = await Database.Data.VoiceLink.GetChannelRowAsync(guild.Id, voiceChannel.Id);
            VoiceLinkRow metaRow = await Database.Data.VoiceLink.GetRowAsync(guild.Id);

            ITextChannel textChannel = null;
            try
            {
                textChannel = guild.GetTextChannel(channelRow.TextChannelId);
            }
            catch {}

            if (connectedUsers.Count(x => !x.IsBot) == 0 && metaRow.DeleteChannels)
            {
                if(textChannel is null) return;
                await textChannel.DeleteAsync();
                channelRow.TextChannelId = 0;
                await Database.Data.VoiceLink.SaveChannelRowAsync(channelRow);
                return;
            }

            if (textChannel is null)
            {
                RestTextChannel restTextChannel = await guild.CreateTextChannelAsync($"{metaRow.Prefix.Value}{voiceChannel.Name}", x =>
                {
                    if (voiceChannel.CategoryId.HasValue) x.CategoryId = voiceChannel.CategoryId.Value;
                    x.Topic = $"Users in {voiceChannel.Name} have access - Created by Utili";
                    x.PermissionOverwrites = new List<Overwrite>
                    {
                        new Overwrite(_client.CurrentUser.Id, PermissionTarget.User,
                            new OverwritePermissions(viewChannel: PermValue.Allow)),
                        new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role,
                            new OverwritePermissions(viewChannel: PermValue.Deny))
                    };
                });

                channelRow.TextChannelId = restTextChannel.Id;
                await Database.Data.VoiceLink.SaveChannelRowAsync(channelRow);
                textChannel = restTextChannel;
            }

            if(!textChannel.BotHasPermissions(ChannelPermission.ViewChannel, ChannelPermission.ManageChannels, ChannelPermission.ManageRoles)) return;

            List<Overwrite> overwrites = textChannel.PermissionOverwrites.ToList();
            bool overwritesChanged = false;

            overwrites.RemoveAll(x =>
            {
                if (x.TargetType == PermissionTarget.User && x.TargetId != _client.CurrentUser.Id)
                {
                    SocketGuildUser existingUser = guild.GetUser(x.TargetId);
                    if (existingUser?.VoiceChannel is null || existingUser.VoiceChannel.Id != voiceChannel.Id)
                    {
                        overwritesChanged = true;
                        return true;
                    }
                }
                return false;
            });

            foreach (SocketGuildUser connectedUser in connectedUsers)
            {
                if (!textChannel.GetPermissionOverwrite(connectedUser).HasValue)
                {
                    overwritesChanged = true;
                    overwrites.Add(new Overwrite(connectedUser.Id, PermissionTarget.User,
                        new OverwritePermissions(viewChannel: PermValue.Allow)));
                }
            }

            OverwritePermissions? everyonePermissions = textChannel.GetPermissionOverwrite(guild.EveryoneRole);

            if (!everyonePermissions.HasValue || everyonePermissions.Value.ViewChannel != PermValue.Deny)
            {
                overwritesChanged = true;
                overwrites.Add(new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role,
                    new OverwritePermissions(viewChannel: PermValue.Deny)));
            }

            if (overwritesChanged)
            {
                await textChannel.ModifyAsync(x => x.PermissionOverwrites = overwrites);
            }
        }
    }
}
