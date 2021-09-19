using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NewDatabase.Entities;
using NewDatabase.Extensions;
using Utili.Extensions;

namespace Utili.Services
{
    public class JoinRolesService
    {
        private readonly ILogger<JoinRolesService> _logger;
        private readonly DiscordClientBase _client;
        private readonly IServiceScopeFactory _scopeFactory;
        
        private Dictionary<(ulong, ulong), Timer> _pendingTimers = new();
        
        public JoinRolesService(ILogger<JoinRolesService> logger, DiscordClientBase client, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _client = client;
            _scopeFactory = scopeFactory;
        }

        public void Start()
        {
            _ = ScheduleAllAddRoles();
        }

        public async Task MemberJoined(IServiceScope scope, MemberJoinedEventArgs e)
        {
            try
            {
                var db = scope.GetDbContext();
                var config = await db.JoinRolesConfigurations.GetForGuildAsync(e.GuildId);
                IGuild guild = _client.GetGuild(e.GuildId);

                if (config.WaitForVerification && (e.Member.IsPending || guild.VerificationLevel >= GuildVerificationLevel.High))
                {
                    var memberRecord = await db.JoinRolesPendingMembers.GetForMemberAsync(e.GuildId, e.Member.Id);
                    if (memberRecord is null)
                    {
                        memberRecord = new JoinRolesPendingMember(e.GuildId, e.Member.Id)
                        {
                            IsPending = e.Member.IsPending,
                            ScheduledFor = guild.VerificationLevel >= GuildVerificationLevel.High
                                ? DateTime.UtcNow.AddMinutes(10)
                                : DateTime.MinValue
                        };
                        db.JoinRolesPendingMembers.Add(memberRecord);
                    }
                    else
                    {
                        memberRecord.IsPending = e.Member.IsPending;
                        memberRecord.ScheduledFor = guild.VerificationLevel >= GuildVerificationLevel.High
                            ? DateTime.UtcNow.AddMinutes(10)
                            : DateTime.MinValue;
                        db.JoinRolesPendingMembers.Update(memberRecord);
                    }
                    
                    await db.SaveChangesAsync();
                    
                    if (guild.VerificationLevel >= GuildVerificationLevel.High)
                        ScheduleAddRoles(e.GuildId, e.Member.Id, DateTime.UtcNow.AddMinutes(10));
                    
                    return;
                }

                await AddRolesAsync(e.GuildId, e.Member.Id, false, config.JoinRoles);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on member joined");
            }
        }

        public async Task MemberUpdated(IServiceScope scope, MemberUpdatedEventArgs e)
        {
            try
            {
                var guildId = e.NewMember.GuildId;

                var db = scope.GetDbContext();

                var memberRecord = await db.JoinRolesPendingMembers.GetForMemberAsync(guildId, e.MemberId);
                if (memberRecord is null) return;

                var record = await db.JoinRolesConfigurations.GetForGuildAsync(guildId);
                if (record is null) return;
                
                if (e.NewMember.RoleIds.Any())
                {
                    // The member has been given a role, so all delays are now irrelevant
                    db.JoinRolesPendingMembers.Remove(memberRecord);
                    await db.SaveChangesAsync();
                    await AddRolesAsync(guildId, e.NewMember.Id, false, record.JoinRoles);
                }
                
                if (memberRecord.IsPending && !e.NewMember.IsPending)
                {
                    // The member has completed membership screening

                    if (memberRecord.ScheduledFor < DateTime.UtcNow)
                    {
                        // ... and they have waited for 10 minutes (or the 10 minute wait is disabled)
                        db.JoinRolesPendingMembers.Remove(memberRecord);
                        await db.SaveChangesAsync();
                        await AddRolesAsync(guildId, e.NewMember.Id, false, record.JoinRoles);
                    }
                    else
                    {
                        // ... but they still have to wait for 10 minutes before getting their roles
                        memberRecord.IsPending = false;
                        db.JoinRolesPendingMembers.Update(memberRecord);
                        await db.SaveChangesAsync();
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on member updated");
            }
        }

        private async Task AddRolesAsync(ulong guildId, ulong memberId, bool fromDelay, List<ulong> joinRoles = null)
        {
            if (fromDelay || joinRoles is null)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.GetDbContext();
                
                if (fromDelay)
                {
                    var pendingRecord = await db.JoinRolesPendingMembers.GetForMemberAsync(guildId, memberId);
                    // If the member is yet to complete membership screening they will be granted their roles when they do so.
                    if (pendingRecord.IsPending) return;
                    db.JoinRolesPendingMembers.Remove(pendingRecord);
                    await db.SaveChangesAsync();
                }
                
                joinRoles ??= (await db.JoinRolesConfigurations.GetForGuildAsync(guildId)).JoinRoles;
            }

            IGuild guild = _client.GetGuild(guildId);

            var roles = joinRoles.Select(x => guild.GetRole(x)).ToList();
            roles.RemoveAll(x => x is null || !x.CanBeManaged());
            
            var roleIds = roles.Select(x => x.Id).ToList();
            roleIds = roleIds.Distinct().ToList();

            foreach (var roleId in roleIds)
            {
                await guild.GrantRoleAsync(memberId, roleId, new DefaultRestRequestOptions{Reason = "Join Roles"});
            }
        }

        private async Task ScheduleAllAddRoles()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.GetDbContext();
            var records = await db.JoinRolesPendingMembers.ToListAsync();
            
            foreach(var record in records)
            {
                try
                {
                    ScheduleAddRoles(record.GuildId, record.MemberId, record.ScheduledFor);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Exception thrown re-scheduling pending join role user {record.GuildId}/{record.GuildId} (for {record.ScheduledFor})");
                }
            }

            if(records.Count > 0) _logger.LogInformation($"Re-scheduled {records.Count} pending join role user{(records.Count == 1 ? "" : "s")}");
        }

        private void ScheduleAddRoles(ulong guildId, ulong memberId, DateTime due)
        {
            lock (_pendingTimers)
            {
                var key = (guildId, memberId);
                if (_pendingTimers.TryGetValue(key, out var timer))
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
