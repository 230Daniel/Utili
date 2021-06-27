using System;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
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

        public async Task MemberJoined(MemberJoinedEventArgs e)
        {
            try
            {
                if (e.Member.IsBot) return;

                IGuild guild = _client.GetGuild(e.GuildId);
                var row = await RolePersist.GetRowAsync(e.GuildId);
                if (!row.Enabled) return;

                var persistRow = await RolePersist.GetPersistRowAsync(e.GuildId, e.Member.Id);

                var roles = persistRow.Roles.Select(x => guild.GetRole(x)).ToList();
                roles.RemoveAll(x => x is null || !x.CanBeManaged() || row.ExcludedRoles.Contains(x.Id));
                
                IMember member = guild.GetMember(e.Member.Id);
                member ??= await guild.FetchMemberAsync(e.Member.Id);
                
                var roleIds = roles.Select(x => x.Id).ToList();
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

        public async Task MemberLeft(MemberLeftEventArgs e, CachedMember member)
        {
            try
            {
                if (e.User.IsBot) return;
                
                IGuild guild = _client.GetGuild(e.GuildId);

                var row = await RolePersist.GetRowAsync(e.GuildId);
                if(!row.Enabled) return;
                
                if (member is null) throw new Exception($"Member {e.User.Id} was not cached in guild {e.GuildId}");

                var persistRow = await RolePersist.GetPersistRowAsync(guild.Id, e.User.Id);
                
                persistRow.Roles.AddRange(member.RoleIds.Select(x => x.RawValue));
                persistRow.Roles = persistRow.Roles.Distinct().ToList();

                await RolePersist.SavePersistRowAsync(persistRow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on member left");
            }
        }
    }
}
