using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord.WebSocket;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
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

                foreach (ulong roleId in persistRow.Roles.Distinct().Where(x => !row.ExcludedRoles.Contains(x)))
                {
                    IRole role = guild.GetRole(roleId);
                    if (role is not null && role.CanBeManaged())
                    {
                        await e.Member.GrantRoleAsync(roleId);
                        await Task.Delay(1000);
                    }
                }

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

                persistRow.Roles.AddRange((e.User as IMember).GetRoles().Values.Where(x => x.Id != e.GuildId).Select(x => x.Id.RawValue));

                await RolePersist.SavePersistRowAsync(persistRow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on member left");
            }
        }
    }
}
