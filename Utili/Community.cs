using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database.Data;
using Discord.WebSocket;
using static Utili.Program;

namespace Utili
{
    internal static class Community
    {
        private static Timer _roleTimer;

        public static void Initialise()
        {
            if (!_config.Production) return;
            _roleTimer?.Dispose();

            _roleTimer = new Timer(60000);
            _roleTimer.Elapsed += RoleTimer_Elapsed;
            _roleTimer.Start();
        }

        private static void RoleTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _roleTimer.Stop();
            try
            {
                AddRolesAsync().GetAwaiter().GetResult();
                RemoveRolesAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.ReportError("Com R", ex);
            }
            _roleTimer.Start();
        }

        private static async Task AddRolesAsync()
        {
            SocketGuild guild = _client.GetGuild(_config.Community.GuildId);

            if (guild.Roles.Any(x => x.Id == _config.Community.UserRoleId))
            {
                List<UserRow> userRows = await Users.GetRowsAsync();
                List<SocketGuildUser> users = guild.Users.Where(x => userRows.Any(y => y.UserId == x.Id)).ToList();
                users = users.Where(x => x.Roles.All(y => y.Id != _config.Community.UserRoleId)).ToList();

                SocketRole role = guild.GetRole(_config.Community.UserRoleId);
                foreach (SocketGuildUser user in users)
                {
                    await user.AddRoleAsync(role);
                    await Task.Delay(1000);
                }
            }

            if (guild.Roles.Any(x => x.Id == _config.Community.PremiumRoleId))
            {
                List<SubscriptionsRow> subscriptionRows = await Subscriptions.GetRowsAsync(onlyValid: true);
                List<SocketGuildUser> users = guild.Users.Where(x => subscriptionRows.Any(y => y.UserId == x.Id)).ToList();
                users = users.Where(x => x.Roles.All(y => y.Id != _config.Community.PremiumRoleId)).ToList();

                SocketRole role = guild.GetRole(_config.Community.PremiumRoleId);
                foreach (SocketGuildUser user in users)
                {
                    await user.AddRoleAsync(role);
                    await Task.Delay(1000);
                }
            }
        }

        private static async Task RemoveRolesAsync()
        {
            SocketGuild guild = _client.GetGuild(_config.Community.GuildId);

            if (guild.Roles.Any(x => x.Id == _config.Community.UserRoleId))
            {
                List<UserRow> userRows = await Users.GetRowsAsync();
                List<SocketGuildUser> users = guild.Users.Where(x => userRows.All(y => y.UserId != x.Id)).ToList();
                users = users.Where(x => x.Roles.Any(y => y.Id == _config.Community.UserRoleId)).ToList();

                SocketRole role = guild.GetRole(_config.Community.UserRoleId);
                foreach (SocketGuildUser user in users)
                {
                    await user.RemoveRoleAsync(role);
                    await Task.Delay(1000);
                }
            }

            if (guild.Roles.Any(x => x.Id == _config.Community.PremiumRoleId))
            {
                List<SubscriptionsRow> subscriptionRows = await Subscriptions.GetRowsAsync(onlyValid: true);
                List<SocketGuildUser> users = guild.Users.Where(x => subscriptionRows.All(y => y.UserId != x.Id)).ToList();
                users = users.Where(x => x.Roles.Any(y => y.Id == _config.Community.PremiumRoleId)).ToList();

                SocketRole role = guild.GetRole(_config.Community.PremiumRoleId);
                foreach (SocketGuildUser user in users)
                {
                    await user.RemoveRoleAsync(role);
                    await Task.Delay(1000);
                }
            }
        }
    }
}
