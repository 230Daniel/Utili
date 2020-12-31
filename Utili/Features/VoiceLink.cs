using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database.Data;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Utili.Features
{
    internal static class VoiceLink
    {
        private static Timer _timer;
        private static List<SocketVoiceChannel> _channelsRequiringUpdate = new List<SocketVoiceChannel>();

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
            List<SocketVoiceChannel> channelsToUpdate = new List<SocketVoiceChannel>();

            lock (_channelsRequiringUpdate)
            {
                foreach (SocketVoiceChannel voiceChannel in _channelsRequiringUpdate)
                {
                    if (channelsToUpdate.All(x => x.Id != voiceChannel.Id))
                    {
                        channelsToUpdate.Add(voiceChannel);
                    }
                }

                _channelsRequiringUpdate.Clear();
            }

            List<Task> tasks = channelsToUpdate.Select(UpdateLinkedChannelAsync).ToList();
            await Task.WhenAll(tasks);
        }

        private static async Task UpdateLinkedChannelAsync(SocketVoiceChannel voiceChannel)
        {
            await Task.Delay(1);

            SocketGuild guild = voiceChannel.Guild;

            if (BotPermissions.IsMissingPermissions(voiceChannel, new[] {ChannelPermission.ManageChannels}, out _))
            {
                return;
            }

            List<SocketGuildUser> connectedUsers =
                guild.Users.Where(x => x.VoiceChannel != null && x.VoiceChannel.Id == voiceChannel.Id).ToList();

            VoiceLinkChannelRow channelRow = await Database.Data.VoiceLink.GetChannelRowAsync(guild.Id, voiceChannel.Id);
            VoiceLinkRow metaRow = await Database.Data.VoiceLink.GetRowAsync(guild.Id);

            ITextChannel textChannel = null;
            try
            {
                textChannel = guild.GetTextChannel(channelRow.TextChannelId);
            }
            catch {}

            if (connectedUsers.Count(x => !x.IsBot) == 0 && textChannel != null && metaRow.DeleteChannels)
            {
                await textChannel.DeleteAsync();
                channelRow.TextChannelId = 0;
                await Database.Data.VoiceLink.SaveChannelRowAsync(channelRow);
                return;
            }

            if (connectedUsers.Count(x => !x.IsBot) == 0 && metaRow.DeleteChannels)
            {
                return;
            }

            if (textChannel == null)
            {
                RestTextChannel restTextChannel = await guild.CreateTextChannelAsync($"{metaRow.Prefix.Value}{voiceChannel.Name}", x =>
                {
                    if (voiceChannel.CategoryId.HasValue) x.CategoryId = voiceChannel.CategoryId.Value;
                    x.Topic = $"Users in {voiceChannel.Name} have access - Created by Utili";
                    x.PermissionOverwrites = new List<Overwrite>
                    {
                        new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role,
                            new OverwritePermissions(viewChannel: PermValue.Deny))
                    };
                });

                channelRow.TextChannelId = restTextChannel.Id;

                await Database.Data.VoiceLink.SaveChannelRowAsync(channelRow);

                textChannel = restTextChannel;
            }

            foreach(Overwrite existingOverwrite in textChannel.PermissionOverwrites)
            {
                if (existingOverwrite.TargetType == PermissionTarget.User)
                {
                    try
                    {
                        SocketGuildUser existingUser = guild.GetUser(existingOverwrite.TargetId);
                        if (existingUser?.VoiceChannel == null || existingUser.VoiceChannel.Id != voiceChannel.Id)
                        {
                            await textChannel.RemovePermissionOverwriteAsync(existingUser);
                        }
                    }
                    catch {}
                }
            }

            foreach (SocketGuildUser connectedUser in connectedUsers)
            {
                if (!textChannel.GetPermissionOverwrite(connectedUser).HasValue)
                {
                    await textChannel.AddPermissionOverwriteAsync(connectedUser,
                        new OverwritePermissions(viewChannel: PermValue.Allow));
                }
            }

            OverwritePermissions? everyonePermissions = textChannel.GetPermissionOverwrite(guild.EveryoneRole);

            if (!everyonePermissions.HasValue || everyonePermissions.Value.ViewChannel != PermValue.Deny)
            {
                await textChannel.AddPermissionOverwriteAsync(guild.EveryoneRole,
                    new OverwritePermissions(viewChannel: PermValue.Deny));
            }
        }
    }
}
