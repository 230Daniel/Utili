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
    class VoiceRoles
    {
        private Timer _timer;
        private bool _processingNow;
        private bool _safeToRequestUpdate = true;
        private List<VoiceUpdateRequest> _updateRequests = new List<VoiceUpdateRequest>();

        public void Start()
        {
            _timer?.Dispose();

            _timer = new Timer(2500);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        public void RequestUpdate(SocketGuildUser user, SocketVoiceState before, SocketVoiceState after)
        {
            while (!_safeToRequestUpdate)
            {
                Task.Delay(20).GetAwaiter().GetResult();
            }

            _updateRequests.Add(new VoiceUpdateRequest(user, before, after));
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_processingNow)
            {
                return;
            }

            _processingNow = true;

            UpdateUsers().GetAwaiter().GetResult();

            _processingNow = false;
        }

        private async Task UpdateUsers()
        {
            List<VoiceUpdateRequest> requests = new List<VoiceUpdateRequest>();
            _safeToRequestUpdate = false;

            foreach (VoiceUpdateRequest request in _updateRequests)
            {
                if (requests.Select(x => x.User.Id).Contains(request.User.Id))
                {
                    VoiceUpdateRequest existingRequest = requests.First(x => x.User.Id == request.User.Id);
                    existingRequest.After = request.After;
                }
                else
                {
                    requests.Add(request);
                }
            }

            _updateRequests.Clear();
            _safeToRequestUpdate = true;

            foreach (VoiceUpdateRequest request in requests)
            {
                try
                {
                    await UpdateUser(request);
                }
                catch {}
            }
        }

        private async Task UpdateUser(VoiceUpdateRequest request)
        {
            SocketGuildUser user = request.User;
            SocketGuild guild = user.Guild;

            if(!Helper.RequiresUpdate(request.Before, request.After))
            {
                return;
            }

            List<VoiceRolesRow> rows = Database.Data.VoiceRoles.GetRows(guild.Id);

            SocketRole beforeRole = guild.EveryoneRole;
            SocketRole afterRole = guild.EveryoneRole;

            if (request.Before.VoiceChannel != null && rows.Count(x => x.ChannelId == request.Before.VoiceChannel.Id) > 0)
            {
                VoiceRolesRow row = rows.First(x => x.ChannelId == request.Before.VoiceChannel.Id);
                beforeRole = guild.GetRole(row.RoleId);
            }

            if (request.After.VoiceChannel != null && rows.Count(x => x.ChannelId == request.After.VoiceChannel.Id) > 0)
            {
                VoiceRolesRow row = rows.First(x => x.ChannelId == request.After.VoiceChannel.Id);
                afterRole = guild.GetRole(row.RoleId);
            }

            if (beforeRole.Id == afterRole.Id)
            {
                return;
            }

            if (beforeRole != guild.EveryoneRole && BotPermissions.CanManageRole(beforeRole))
            {
                await user.RemoveRoleAsync(beforeRole);
            }

            if (afterRole != guild.EveryoneRole && BotPermissions.CanManageRole(afterRole))
            {
                await user.AddRoleAsync(afterRole);
            }
        }
    }

    internal class VoiceUpdateRequest
    {
        public SocketGuildUser User { get; set; }
        public SocketVoiceState Before { get; set; }
        public SocketVoiceState After { get; set; }

        public VoiceUpdateRequest(SocketGuildUser user, SocketVoiceState before, SocketVoiceState after)
        {
            User = user;
            Before = before;
            After = after;
        }
    }
}
