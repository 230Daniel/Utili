using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database.Data;
using Discord;
using Discord.WebSocket;
using static Utili.Program;

namespace Utili.Features
{
    internal class InactiveRole
    {
        private Timer _timer;

        public async Task UpdateUserAsync(SocketGuild guild, SocketGuildUser user)
        {
            InactiveRoleRow row = Database.Data.InactiveRole.GetRows(guild.Id).FirstOrDefault();

            if(guild.Roles.Any(x => x.Id == row.RoleId))
            {
                Database.Data.InactiveRole.UpdateUser(guild.Id, user.Id);

                if (user.Roles.Any(x => x.Id == row.RoleId))
                {
                    await user.RemoveRoleAsync(guild.GetRole(row.RoleId));
                }
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
            UpdateGuildsAsync().GetAwaiter().GetResult();
        }

        private async Task UpdateGuildsAsync()
        {
            List<InactiveRoleRow> guildsRequiringUpdate = Database.Data.InactiveRole.GetUpdateRequiredRows().Take(5).ToList();

            guildsRequiringUpdate.ForEach(x =>
            {
                x.LastUpdate = DateTime.UtcNow;
                Database.Data.InactiveRole.SaveLastUpdate(x);
            });

            List<Task> tasks = new List<Task>();

            foreach (InactiveRoleRow row in guildsRequiringUpdate.Where(x => _client.Guilds.Any(y => y.Id == x.GuildId)))
            {
                SocketGuild guild = _client.GetGuild(row.GuildId);
                tasks.Add(UpdateGuildAsync(row, guild));
            }

            await Task.WhenAll(tasks);
        }

        private async Task UpdateGuildAsync(InactiveRoleRow row, SocketGuild guild)
        {
            SocketRole inactiveRole = guild.GetRole(row.RoleId);

            if (!BotPermissions.CanManageRole(inactiveRole))
            {
                return;
            }

            List<IGuildUser> users = (await guild.GetUsersAsync().FlattenAsync()).ToList();
            IGuildUser bot = users.First(x => x.Id == _client.CurrentUser.Id);
            List<InactiveRoleUserRow> userRows = Database.Data.InactiveRole.GetUsers(guild.Id);

            foreach (IGuildUser user in users.Where(x => !x.IsBot).OrderBy(x => x.Id))
            {
                // DefaultLastAction is set to the time when the activity data started being recorded
                DateTime lastAction = row.DefaultLastAction;

                if (bot.JoinedAt.HasValue && bot.JoinedAt > lastAction) 
                    lastAction = bot.JoinedAt.Value.UtcDateTime;

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
