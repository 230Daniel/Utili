using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using Discord.WebSocket;
using Database.Data;
using Discord.Rest;
using static Utili.Program;

namespace Utili.Features
{
    internal static class JoinRoles
    {
        private static Timer _pendingRolesTimer;

        public static void Start()
        {
            _pendingRolesTimer?.Dispose();
            _pendingRolesTimer = new Timer(10000);
            _pendingRolesTimer.Elapsed += PendingRolesTimer_Elapsed;
            _pendingRolesTimer.Start();
        }

        public static async Task UserJoined(SocketGuildUser user, bool alwaysAdd = false)
        {
            SocketGuild guild = user.Guild;
            JoinRolesRow row = await Database.Data.JoinRoles.GetRowAsync(guild.Id);

            if (row.WaitForVerification && !alwaysAdd)
            {
                if (user.IsPending.HasValue && user.IsPending.Value)
                {
                    // Their roles will be added on GuildUserUpdated
                    return;
                }

                if ((int) guild.VerificationLevel >= 3)
                {
                    // Their roles will be added after 10 minutes
                    MiscRow pendingRow = new MiscRow(guild.Id, "JoinRoles-Pending", $"{DateTime.UtcNow + TimeSpan.FromMinutes(10)}///{user.Id}");
                    await Misc.SaveRowAsync(pendingRow);
                    return;
                }
            }

            foreach (ulong roleId in row.JoinRoles.Take(5))
            {
                SocketRole role = guild.GetRole(roleId);
                if (role is not null && BotPermissions.CanManageRole(role))
                {
                    await user.AddRoleAsync(role);
                    await Task.Delay(1000);
                }
            }
        }

        private static void PendingRolesTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = GivePendingRolesAsync();
        }

        private static async Task GivePendingRolesAsync()
        {
            List<MiscRow> pendingRows = await Misc.GetRowsAsync(type: "JoinRoles-Pending");
            pendingRows.RemoveAll(x => DateTime.UtcNow < DateTime.Parse(x.Value.Split("///")[0]));
            pendingRows.RemoveAll(x => _client.Guilds.All(y => y.Id != x.GuildId));
            pendingRows.ForEach(x => _ = x.DeleteAsync());

            foreach (MiscRow pendingRow in pendingRows)
            {
                try
                {
                    SocketGuild guild = _client.GetGuild(pendingRow.GuildId);
                    JoinRolesRow row = await Database.Data.JoinRoles.GetRowAsync(guild.Id);
                    if(row.JoinRoles.Count == 0) return;

                    RestGuildUser user = await _rest.GetGuildUserAsync(guild.Id, ulong.Parse(pendingRow.Value.Split("///")[1]));
                    
                    foreach (ulong roleId in row.JoinRoles.Take(5))
                    {
                        SocketRole role = guild.GetRole(roleId);
                        if (role is not null && BotPermissions.CanManageRole(role))
                        {
                            await user.AddRoleAsync(role);
                            await Task.Delay(1000);
                        }
                    }
                }
                catch { }
            }
        }
    }
}
