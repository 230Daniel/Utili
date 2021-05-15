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

        Dictionary<(ulong, ulong), Timer> _pendingTimers = new();

        public JoinRolesService(ILogger<JoinRolesService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;
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

                if (row.WaitForVerification && (e.Member.IsPending || guild.VerificationLevel >= GuildVerificationLevel.High))
                {
                    JoinRolesPendingRow pendingRow = await JoinRoles.GetPendingRowAsync(e.GuildId, e.Member.Id);
                    pendingRow.IsPending = e.Member.IsPending;
                    if (guild.VerificationLevel >= GuildVerificationLevel.High)
                    {
                        pendingRow.ScheduledFor = DateTime.UtcNow.AddMinutes(10);
                        ScheduleAddRoles(e.GuildId, e.Member.Id, DateTime.UtcNow.AddMinutes(10));
                    }
                    await pendingRow.SaveAsync();
                    return;
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
                Snowflake guildId = e.NewMember.GuildId;

                JoinRolesRow row = await JoinRoles.GetRowAsync(guildId);
                JoinRolesPendingRow pendingRow = await JoinRoles.GetPendingRowAsync(guildId, e.MemberId);
                if (pendingRow.New) return;
                
                if (e.NewMember.RoleIds.Any())
                {
                    // The member has been given a role, so all delays are now irrelevant
                    await pendingRow.DeleteAsync();
                    await AddRolesAsync(guildId, e.NewMember.Id, false, row);
                }
                
                if (pendingRow.IsPending && !e.NewMember.IsPending)
                {
                    // The member has completed membership screening

                    if (pendingRow.ScheduledFor < DateTime.UtcNow)
                    {
                        // ... and they have waited for 10 minutes (or the 10 minute wait is disabled)
                        await pendingRow.DeleteAsync();
                        await AddRolesAsync(guildId, e.NewMember.Id, false, row);
                        return;
                    }
                    
                    // ... but they still have to wait for 10 minutes before getting their roles
                    pendingRow.IsPending = false;
                    await pendingRow.SaveAsync();
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on member updated");
            }
        }

        async Task AddRolesAsync(ulong guildId, ulong memberId, bool fromDelay, JoinRolesRow row = null)
        {
            if (fromDelay)
            {
                JoinRolesPendingRow pendingRow = await JoinRoles.GetPendingRowAsync(guildId, memberId);
                // If the member is yet to complete membership screening they will be granted their roles when they do so.
                if (pendingRow.IsPending) return;
                await pendingRow.DeleteAsync();
            }
            
            row ??= await JoinRoles.GetRowAsync(guildId);
            IGuild guild = _client.GetGuild(guildId);

            IMember member = guild.GetMember(memberId);
            member ??= await guild.FetchMemberAsync(memberId);
            
            List<IRole> roles = row.JoinRoles.Select(x => guild.GetRole(x)).ToList();
            roles.RemoveAll(x => x is null || !x.CanBeManaged());
            
            List<Snowflake> roleIds = roles.Select(x => x.Id).ToList();
            roleIds.AddRange(member.RoleIds);
            roleIds = roleIds.Distinct().ToList();
            
            await guild.ModifyMemberAsync(memberId, x => x.RoleIds = roleIds);
        }
        
        async Task ScheduleAllAddRoles()
        {
            List<JoinRolesPendingRow> rows = await JoinRoles.GetPendingRowsAsync();
            foreach(JoinRolesPendingRow row in rows)
            {
                try
                {
                    ScheduleAddRoles(row.GuildId, row.UserId, row.ScheduledFor);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Exception thrown re-scheduling pending join role user {row.GuildId}/{row.GuildId} (for {row.ScheduledFor})");
                }
            }

            if(rows.Count > 0) _logger.LogInformation($"Re-scheduled {rows.Count} pending join role user{(rows.Count == 1 ? "" : "s")}");
        }

        void ScheduleAddRoles(ulong guildId, ulong memberId, DateTime due)
        {
            lock (_pendingTimers)
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
}
