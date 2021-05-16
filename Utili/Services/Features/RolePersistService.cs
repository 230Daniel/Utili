using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Disqord.Rest.Default;
using Microsoft.Extensions.Logging;
using Utili.Extensions;

namespace Utili.Services
{
    public class RolePersistService
    {
        ILogger<RolePersistService> _logger;
        DiscordClientBase _client;

        public RolePersistService(ILogger<RolePersistService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task MemberJoined(MemberJoinedEventArgs e)
        {
            try
            {
                if (e.Member.IsBot) return;

                IGuild guild = _client.GetGuild(e.GuildId);
                RolePersistRow row = await RolePersist.GetRowAsync(e.GuildId);
                if (!row.Enabled) return;

                RolePersistRolesRow persistRow = await RolePersist.GetPersistRowAsync(e.GuildId, e.Member.Id);

                List<IRole> roles = persistRow.Roles.Select(x => guild.GetRole(x)).ToList();
                roles.RemoveAll(x => x is null || !x.CanBeManaged() || row.ExcludedRoles.Contains(x.Id));
                
                IMember member = guild.GetMember(e.Member.Id);
                member ??= await guild.FetchMemberAsync(e.Member.Id);
                
                List<Snowflake> roleIds = roles.Select(x => x.Id).ToList();
                roleIds.AddRange(member.RoleIds);
                roleIds = roleIds.Distinct().ToList();
            
                await e.Member.ModifyAsync(x => x.RoleIds = roleIds, new DefaultRestRequestOptions{Reason = "Role Persist"});
                
                await RolePersist.DeletePersistRowAsync(persistRow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on member joined");
            }
        }

        public async Task MemberLeft(MemberLeftEventArgs e)
        {
            try
            {
                if (e.User.IsBot) return;

                IGuild guild = _client.GetGuild(e.GuildId);
                RolePersistRow row = await RolePersist.GetRowAsync(e.GuildId);
                if(!row.Enabled) return;

                RolePersistRolesRow persistRow = await RolePersist.GetPersistRowAsync(guild.Id, e.User.Id);

                RoleCacheRow roleCache = await RoleCache.GetRowAsync(e.GuildId, e.User.Id);
                persistRow.Roles.AddRange(roleCache.RoleIds);

                await RolePersist.SavePersistRowAsync(persistRow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on member left");
            }
        }
    }
}
