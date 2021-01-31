using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database.Data;
using Discord.WebSocket;

namespace Utili.Features
{
    internal static class VoiceRoles
    {
        private static Timer _timer;
        private static List<VoiceUpdateRequest> _updateRequests = new List<VoiceUpdateRequest>();

        public static void Start()
        {
            _timer?.Dispose();

            _timer = new Timer(2500);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        public static void RequestUpdate(SocketGuildUser user, SocketVoiceState before, SocketVoiceState after)
        {
            lock (_updateRequests)
            {
                _updateRequests.Add(new VoiceUpdateRequest(user, before, after));
            }
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateUsersAsync().GetAwaiter().GetResult();
        }

        private static async Task UpdateUsersAsync()
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

        private static async Task UpdateUserAsync(VoiceUpdateRequest request)
        {
            await Task.Delay(1);

            SocketGuildUser user = request.User;
            SocketGuild guild = user.Guild;

            if(!Helper.RequiresUpdate(request.Before, request.After)) return;

            List<VoiceRolesRow> rows = await Database.Data.VoiceRoles.GetRowsAsync(guild.Id);
            rows = rows.Where(x => guild.Roles.Any(y => y.Id == x.RoleId)).ToList();

            SocketRole beforeRole = guild.EveryoneRole;
            SocketRole afterRole = guild.EveryoneRole;

            if (request.Before.VoiceChannel is not null)
            {
                if (rows.Any(x => x.ChannelId == request.Before.VoiceChannel.Id))
                {
                    VoiceRolesRow row = rows.First(x => x.ChannelId == request.Before.VoiceChannel.Id);
                    beforeRole = guild.GetRole(row.RoleId);
                }
                else if (rows.Any(x => x.ChannelId == 0))
                {
                    VoiceRolesRow row = rows.First(x => x.ChannelId == 0);
                    beforeRole = guild.GetRole(row.RoleId);
                }
            }

            if (request.After.VoiceChannel is not null)
            {
                if (rows.Any(x => x.ChannelId == request.After.VoiceChannel.Id))
                {
                    VoiceRolesRow row = rows.First(x => x.ChannelId == request.After.VoiceChannel.Id);
                    afterRole = guild.GetRole(row.RoleId);
                }
                else if (rows.Any(x => x.ChannelId == 0))
                {
                    VoiceRolesRow row = rows.First(x => x.ChannelId == 0);
                    afterRole = guild.GetRole(row.RoleId);
                }
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
