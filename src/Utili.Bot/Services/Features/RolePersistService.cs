using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Utili.Database.Entities;
using Utili.Database.Extensions;
using Utili.Bot.Extensions;

namespace Utili.Bot.Services;

public class RolePersistService
{
    private readonly ILogger<RolePersistService> _logger;
    private readonly DiscordClientBase _client;

    public RolePersistService(ILogger<RolePersistService> logger, DiscordClientBase client)
    {
        _logger = logger;
        _client = client;
    }

    // Returns true if it granted any roles to the new member
    public async Task<bool> MemberJoined(IServiceScope scope, MemberJoinedEventArgs e)
    {
        try
        {
            if (e.Member.IsBot) return false;

            var db = scope.GetDbContext();
            var config = await db.RolePersistConfigurations.GetForGuildAsync(e.GuildId);
            if (config is null || !config.Enabled) return false;

            var memberRecord = await db.RolePersistMembers.GetForMemberAsync(e.GuildId, e.Member.Id);
            if (memberRecord is null) return false;

            var guild = _client.GetGuild(e.GuildId);
            var roles = memberRecord.Roles.Select(x => guild.GetRole(x)).ToList();
            roles.RemoveAll(x => x is null || !x.CanBeManaged() || config.ExcludedRoles.Contains(x.Id));
            if (!roles.Any()) return false;

            IMember member = guild.GetMember(e.Member.Id);
            member ??= await guild.FetchMemberAsync(e.Member.Id);

            var roleIds = roles.Select(x => x.Id).ToList();
            roleIds.AddRange(member.RoleIds);
            roleIds = roleIds.Distinct().ToList();

            await e.Member.ModifyAsync(x => x.RoleIds = roleIds, new DefaultRestRequestOptions { Reason = "Role Persist" });

            db.RolePersistMembers.Remove(memberRecord);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown on member joined");
            return false;
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
            if (config is null || !config.Enabled) return;

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