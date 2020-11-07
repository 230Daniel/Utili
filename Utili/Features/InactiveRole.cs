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
using Database;
using Discord.Commands;
using static Utili.Program;

namespace Utili.Features
{
    class InactiveRole
    {
        private Timer _timer;
        private bool _startingProcessing;

        public async Task MessageReceived(SocketCommandContext context)
        {
            if (context.User.IsBot || context.Channel is SocketDMChannel) return;

            InactiveRoleRow row = Database.Data.InactiveRole.GetRows(context.Guild.Id).FirstOrDefault();

            if (context.Guild.Roles.Select(x => x.Id).Contains(row.RoleId))
            {
                Database.Data.InactiveRole.UpdateUser(context.Guild.Id, context.User.Id);
            }
        }

        public void Start()
        {
            _timer?.Dispose();

            _timer = new Timer(30000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_startingProcessing) return;

            _startingProcessing = true;
            UpdateGuilds().GetAwaiter().GetResult();
            _startingProcessing = false;
        }

        private async Task UpdateGuilds()
        {
            List<InactiveRoleRow> guildsRequiringUpdate = Database.Data.InactiveRole.GetUpdateRequiredRows();

            guildsRequiringUpdate.ForEach(x =>
            {
                x.LastUpdate = DateTime.UtcNow;
                Database.Data.InactiveRole.SaveLastUpdate(x);
            });

            foreach (InactiveRoleRow row in guildsRequiringUpdate)
            {
                try
                {
                    SocketGuild guild = _client.GetGuild(row.GuildId);
                    _ = UpdateGuild(row, guild);
                }
                catch{}
            }
        }

        private async Task UpdateGuild(InactiveRoleRow row, SocketGuild guild)
        {
            SocketRole inactiveRole = guild.GetRole(row.RoleId);

            if (!BotPermissions.CanManageRole(inactiveRole))
            {
                return;
            }

            List<IGuildUser> users = (await guild.GetUsersAsync().FlattenAsync()).ToList();
            IGuildUser bot = users.First(x => x.Id == _client.CurrentUser.Id);
            List<InactiveRoleUserRow> userRows = Database.Data.InactiveRole.GetUsers(guild.Id);

            foreach (IGuildUser user in users)
            {
                DateTime lastAction = DateTime.MinValue;
                if (bot.JoinedAt.HasValue) lastAction = bot.JoinedAt.Value.UtcDateTime;

                List<InactiveRoleUserRow> matchingRows = userRows.Where(x => x.UserId == user.Id).ToList();
                if (matchingRows.Count > 0 && matchingRows.First().LastAction > lastAction)
                    lastAction = matchingRows.First().LastAction;

                if (user.JoinedAt.HasValue && user.JoinedAt.Value.UtcDateTime > lastAction)
                    lastAction = user.JoinedAt.Value.UtcDateTime;

                DateTime minimumLastAction = DateTime.UtcNow - row.Threshold;

                if (lastAction <= minimumLastAction && !user.RoleIds.Contains(row.ImmuneRoleId))
                {
                    if (!row.Inverse)
                    {
                        if (!user.RoleIds.Contains(inactiveRole.Id))
                        {
                            await user.AddRoleAsync(inactiveRole);
                            await Task.Delay(500);
                        }
                    }
                    else
                    {
                        if (user.RoleIds.Contains(inactiveRole.Id))
                        {
                            await user.RemoveRoleAsync(inactiveRole);
                            await Task.Delay(500);
                        }
                    }
                }
                else
                {
                    if (!row.Inverse)
                    {
                        if (user.RoleIds.Contains(inactiveRole.Id))
                        {
                            await user.RemoveRoleAsync(inactiveRole);
                            await Task.Delay(500);
                        }
                    }
                    else
                    {
                        if (!user.RoleIds.Contains(inactiveRole.Id))
                        {
                            await user.AddRoleAsync(inactiveRole);
                            await Task.Delay(500);
                        }
                    }
                }
            }
        }
    }
}
