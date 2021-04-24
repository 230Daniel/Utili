using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using Utili.Extensions;

namespace Utili.Services
{
    public class JoinRolesService
    {
        ILogger<JoinRolesService> _logger;
        DiscordClientBase _client;

        Dictionary<(ulong, ulong), Timer> _pendingTimers;

        public JoinRolesService(ILogger<JoinRolesService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;

            _pendingTimers = new Dictionary<(ulong, ulong), Timer>();
        }

        public void Start()
        {
            _ = ScheduleAllAddRoles();
        }

        public async Task MemberJoined(MemberJoinedEventArgs e)
        {
            try
            {
                JoinRolesRow row = await JoinRoles.GetRowAsync(e.GuildId);
                IGuild guild = _client.GetGuild(e.GuildId);

                if (row.WaitForVerification)
                {
                    if(e.Member.IsPending) return;
                    if (guild.VerificationLevel >= GuildVerificationLevel.High)
                    {
                        MiscRow pendingRow = new MiscRow(guild.Id, "JoinRoles-Pending", $"{DateTime.UtcNow.AddMinutes(10)}///{e.Member.Id}");
                        await Misc.SaveRowAsync(pendingRow);
                        ScheduleAddRoles(e.GuildId, e.Member.Id, DateTime.UtcNow.AddMinutes(10));
                        return;
                    }
                }

                await AddRolesAsync(e.GuildId, e.Member.Id, false, row);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on member joined");
            }
        }

        public async Task MemberUpdated(MemberUpdatedEventArgs e)
        {
            try
            {
                if (e.OldMember is not null && e.OldMember.IsPending && !e.NewMember.IsPending)
                    await AddRolesAsync(e.NewMember.GuildId, e.NewMember.Id, false);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on member updated");
            }
        }

        async Task AddRolesAsync(ulong guildId, ulong memberId, bool fromDelay, JoinRolesRow row = null)
        {
            row ??= await JoinRoles.GetRowAsync(guildId);
            IGuild guild = _client.GetGuild(guildId);

            foreach(ulong roleId in row.JoinRoles.Take(5))
            {
                IRole role = guild.GetRole(roleId);
                if (role is not null && role.CanBeManaged())
                {
                    await guild.GrantRoleAsync(memberId, roleId);
                    await Task.Delay(1000);
                }
            }

            if (fromDelay)
            {
                List<MiscRow> pendingRows = await Misc.GetRowsAsync(guildId, "JoinRoles-Pending");
                foreach(MiscRow pendingRow in pendingRows.Where(x => x.Value.Split("///")[1] == memberId.ToString()))
                    await pendingRow.DeleteAsync();
            }
        }

        async Task ScheduleAllAddRoles()
        {
            List<MiscRow> pendingRows = await Misc.GetRowsAsync(type: "JoinRoles-Pending");
            foreach(MiscRow pendingRow in pendingRows)
            {
                try
                {
                    DateTime due = DateTime.Parse(pendingRow.Value.Split("///")[0]);
                    ulong userId = ulong.Parse(pendingRow.Value.Split("///")[1]);
                    ScheduleAddRoles(pendingRow.GuildId, userId, due);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Exception thrown re-scheduling pending join role user {pendingRow.GuildId}/{pendingRow.Value}");
                }
            }

            if(pendingRows.Count > 0) _logger.LogInformation($"Re-scheduled {pendingRows.Count} pending join role user{(pendingRows.Count == 1 ? "" : "s")}");
        }

        void ScheduleAddRoles(ulong guildId, ulong memberId, DateTime due)
        {
            (ulong, ulong) key = (guildId, memberId);
            if (_pendingTimers.TryGetValue(key, out Timer timer))
            {
                timer.Dispose();
                _pendingTimers.Remove(key);
            }

            if (due <= DateTime.UtcNow.AddSeconds(10))
                _ = AddRolesAsync(guildId, memberId, true);
            else
            {
                timer = new Timer(x =>
                {
                    _ = AddRolesAsync(guildId, memberId, true);
                }, this, due - DateTime.UtcNow, Timeout.InfiniteTimeSpan);
                _pendingTimers.Add(key, timer);
            }
        }
    }
}
