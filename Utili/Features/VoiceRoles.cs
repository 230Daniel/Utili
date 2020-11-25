using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database.Data;
using Discord.WebSocket;

namespace Utili.Features
{
    internal class VoiceRoles
    {
        private Timer _timer;
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
            lock (_updateRequests)
            {
                _updateRequests.Add(new VoiceUpdateRequest(user, before, after));
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateUsersAsync().GetAwaiter().GetResult();
        }

        private async Task UpdateUsersAsync()
        {
            List<VoiceUpdateRequest> requests = new List<VoiceUpdateRequest>();

            lock (_updateRequests)
            {
                foreach (VoiceUpdateRequest request in _updateRequests)
                {
                    if (requests.Any(x => x.Identifier == request.Identifier))
                    {
                        VoiceUpdateRequest existingRequest = requests.First(x => x.Identifier == request.Identifier);
                        existingRequest.After = request.After;
                    }
                    else
                    {
                        requests.Add(request);
                    }
                }

                _updateRequests.Clear();
            }

            List<Task> tasks = requests.Select(UpdateUserAsync).ToList();
            await Task.WhenAll(tasks);
        }

        private async Task UpdateUserAsync(VoiceUpdateRequest request)
        {
            await Task.Delay(1);

            SocketGuildUser user = request.User;
            SocketGuild guild = user.Guild;

            if(!Helper.RequiresUpdate(request.Before, request.After)) return;

            List<VoiceRolesRow> rows = Database.Data.VoiceRoles.GetRows(guild.Id);

            SocketRole beforeRole = guild.EveryoneRole;
            SocketRole afterRole = guild.EveryoneRole;

            if (request.Before.VoiceChannel != null && rows.Any(x => x.ChannelId == request.Before.VoiceChannel.Id))
            {
                VoiceRolesRow row = rows.First(x => x.ChannelId == request.Before.VoiceChannel.Id);
                beforeRole = guild.GetRole(row.RoleId);
            }

            if (request.After.VoiceChannel != null && rows.Any(x => x.ChannelId == request.After.VoiceChannel.Id))
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
        public string Identifier => $"{User.Guild.Id}-{User.Id}";

        public VoiceUpdateRequest(SocketGuildUser user, SocketVoiceState before, SocketVoiceState after)
        {
            User = user;
            Before = before;
            After = after;
        }
    }
}
