using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NewDatabase.Entities;
using NewDatabase.Extensions;
using Utili.Extensions;

namespace Utili.Services
{
    public class RolePersistService
    {
        private readonly ILogger<RolePersistService> _logger;
        private readonly DiscordClientBase _client;

        public RolePersistService(ILogger<RolePersistService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task MemberJoined(IServiceScope scope, MemberJoinedEventArgs e)
        {
            try
            {
                if (e.Member.IsBot) return;

                var db = scope.GetDbContext();
                var config = await db.RolePersistConfigurations.GetForGuildAsync(e.GuildId);
                if (config is null || !config.Enabled) return;

                var memberRecord = await db.RolePersistMembers.GetForMemberAsync(e.GuildId, e.Member.Id);

                var guild = _client.GetGuild(e.GuildId);
                var roles = memberRecord.Roles.Select(x => guild.GetRole(x)).ToList();
                roles.RemoveAll(x => x is null || !x.CanBeManaged() || config.ExcludedRoles.Contains(x.Id));
                
                IMember member = guild.GetMember(e.Member.Id);
                member ??= await guild.FetchMemberAsync(e.Member.Id);
                
                var roleIds = roles.Select(x => x.Id).ToList();
                roleIds.AddRange(member.RoleIds);
                roleIds = roleIds.Distinct().ToList();
            
                await e.Member.ModifyAsync(x => x.RoleIds = roleIds, new DefaultRestRequestOptions{Reason = "Role Persist"});

                db.RolePersistMembers.Remove(memberRecord);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on member joined");
            }
        }

        public async Task MemberLeft(IServiceScope scope, MemberLeftEventArgs e, IMember member)
        {
            try
            {
                if (e.User.IsBot) return;
                
                IGuild guild = _client.GetGuild(e.GuildId);

                var db = scope.GetDbContext();
                var config = await db.RolePersistConfigurations.GetForGuildAsync(e.GuildId);
                if(config is null || !config.Enabled) return;
                
                if (member is null) throw new Exception($"Member {e.User.Id} was not cached in guild {e.GuildId}");

                var memberRecord = await db.RolePersistMembers.GetForMemberAsync(guild.Id, e.User.Id);
                if (memberRecord is null)
                {
                    memberRecord = new RolePersistMember(guild.Id, e.User.Id)
                    {
                        Roles = member.RoleIds.Select(x => x.RawValue).ToList()
                    };
                    db.RolePersistMembers.Add(memberRecord);
                    await db.SaveChangesAsync();
                }
                else
                {
                    memberRecord.Roles.AddRange(member.RoleIds.Select(x => x.RawValue));
                    memberRecord.Roles = memberRecord.Roles.Distinct().ToList();
                    db.RolePersistMembers.Update(memberRecord);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on member left");
            }
        }
    }
}
