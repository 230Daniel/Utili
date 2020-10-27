using Database.Data;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static Utili.Program;

namespace Utili.Features
{
    class VoiceLink
    {
        private Timer _timer;
        private bool _processingNow;
        private bool _safeToRequestUpdate = true;
        private List<SocketVoiceChannel> _channelsRequiringUpdate = new List<SocketVoiceChannel>();

        public void Start()
        {
            _timer?.Dispose();

            _timer = new Timer(1000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        public void RequestUpdate(SocketVoiceChannel channel)
        {
            VoiceLinkRow row = Database.Data.VoiceLink.GetRowForChannel(channel.Guild.Id, channel.Id);
            VoiceLinkRow metaRow = Database.Data.VoiceLink.GetMetaRow(channel.Guild.Id);

            bool enabled = metaRow.Enabled;
            if(row != null && row.Excluded) enabled = false;

            if (enabled)
            {
                while (!_safeToRequestUpdate)
                {
                    Task.Delay(20).GetAwaiter().GetResult();
                }

                _channelsRequiringUpdate.Add(channel);
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_processingNow)
            {
                return;
            }

            _processingNow = true;

            UpdateLinkedChannels().GetAwaiter().GetResult();

            _processingNow = false;
        }

        private async Task UpdateLinkedChannels()
        {
            List<SocketVoiceChannel> channelsToUpdate = new List<SocketVoiceChannel>();
            _safeToRequestUpdate = false;

            foreach (SocketVoiceChannel voiceChannel in _channelsRequiringUpdate)
            {
                if (!channelsToUpdate.Select(x => x.Id).Contains(voiceChannel.Id))
                {
                    channelsToUpdate.Add(voiceChannel);
                }
            }

            _channelsRequiringUpdate.Clear();
            _safeToRequestUpdate = true;

            foreach (SocketVoiceChannel voiceChannel in channelsToUpdate)
            {
                try
                {
                    await UpdateLinkedChannel(voiceChannel);
                }
                catch {}
            }
        }

        private async Task UpdateLinkedChannel(SocketVoiceChannel voiceChannel)
        {
            SocketGuild guild = voiceChannel.Guild;

            List<SocketGuildUser> connectedUsers =
                voiceChannel.Users.Where(x => x.VoiceChannel != null && x.VoiceChannel.Id == voiceChannel.Id).ToList();

            VoiceLinkRow row = Database.Data.VoiceLink.GetRowForChannel(guild.Id, voiceChannel.Id);
            VoiceLinkRow metaRow = Database.Data.VoiceLink.GetMetaRow(guild.Id);

            SocketTextChannel textChannel = null;
            try
            {
                textChannel = guild.GetTextChannel(row.TextChannelId);
            }
            catch {}

            if (connectedUsers.Count == 0 && textChannel != null)
            {
                await textChannel.DeleteAsync();
                row.TextChannelId = 0;
                Database.Data.VoiceLink.SaveRow(row);
                return;
            }

            if (textChannel == null)
            {
                RestTextChannel restTextChannel = await guild.CreateTextChannelAsync($"{metaRow.Prefix}{voiceChannel.Name}", x =>
                {
                    if (voiceChannel.CategoryId.HasValue) x.CategoryId = voiceChannel.CategoryId.Value;
                    x.Topic = "Automatically created by Utili";
                });

                row.TextChannelId = restTextChannel.Id;
                Database.Data.VoiceLink.SaveRow(row);

                await Task.Delay(500);

                textChannel = guild.GetTextChannel(row.TextChannelId);
                await textChannel.AddPermissionOverwriteAsync(guild.EveryoneRole,
                    new OverwritePermissions(viewChannel: PermValue.Deny));
            }

            foreach(Overwrite existingOverwrite in textChannel.PermissionOverwrites)
            {
                if (existingOverwrite.TargetType == PermissionTarget.User)
                {
                    SocketGuildUser existingUser = guild.GetUser(existingOverwrite.TargetId);
                    if (existingUser.VoiceChannel == null || existingUser.VoiceChannel.Id != voiceChannel.Id)
                    {
                        await textChannel.RemovePermissionOverwriteAsync(existingUser);
                    }
                }
            }

            foreach (SocketGuildUser connectedUser in connectedUsers)
            {
                if (!voiceChannel.GetPermissionOverwrite(connectedUser).HasValue)
                {
                    await textChannel.AddPermissionOverwriteAsync(connectedUser,
                        new OverwritePermissions(viewChannel: PermValue.Allow));
                }
            }
        }
    }
}
