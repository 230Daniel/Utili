using System;
using System.Collections.Generic;
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

public class RoleLinkingService
{
    private readonly ILogger<RoleLinkingService> _logger;
    private readonly UtiliDiscordBot _bot;
    private readonly IsPremiumService _isPremiumService;

    private List<RoleLinkAction> _actions;

    public RoleLinkingService(ILogger<RoleLinkingService> logger, UtiliDiscordBot bot, IsPremiumService isPremiumService)
    {
        _logger = logger;
        _bot = bot;
        _isPremiumService = isPremiumService;

        _actions = new List<RoleLinkAction>();
    }

    public async Task MemberUpdated(IServiceScope scope, MemberUpdatedEventArgs e)
    {
        try
        {
            IGuild guild = _bot.GetGuild(e.NewMember.GuildId);

            var db = scope.GetDbContext();
            var configs = await db.RoleLinkingConfigurations.GetAllForGuildAsync(guild.Id);
            if (configs.Count == 0) return;

            if (configs.Count > 2)
            {
                var premium = await _isPremiumService.GetIsGuildPremiumAsync(guild.Id);
                if (!premium) configs = configs.Take(2).ToList();
            }

            if (e.OldMember is null) throw new Exception($"Member {e.MemberId} was not cached in guild {e.NewMember.GuildId}");
            var oldRoles = e.OldMember.RoleIds.Select(x => x.RawValue).ToList();
            var newRoles = e.NewMember.RoleIds.Select(x => x.RawValue).ToList();

            var addedRoles = newRoles.Where(x => oldRoles.All(y => y != x)).ToList();
            var removedRoles = oldRoles.Where(x => newRoles.All(y => y != x)).ToList();

            List<ulong> rolesToAdd;
            List<ulong> rolesToRemove;

            lock (_actions)
            {
                var actionsPerformedByBot = _actions.Where(x => x.GuildId == guild.Id && x.UserId == e.NewMember.Id).ToList();
                foreach (var action in actionsPerformedByBot)
                {
                    if (addedRoles.Contains(action.RoleId) && action.ActionType == RoleLinkActionType.Added)
                    {
                        addedRoles.Remove(action.RoleId);
                        _actions.Remove(action);
                    }
                    else if (removedRoles.Contains(action.RoleId) && action.ActionType == RoleLinkActionType.Removed)
                    {
                        removedRoles.Remove(action.RoleId);
                        _actions.Remove(action);
                    }
                }

                rolesToAdd = configs.Where(x => addedRoles.Contains(x.RoleId) && x.Mode == RoleLinkingMode.GrantOnGrant).Select(x => x.LinkedRoleId).ToList();
                rolesToAdd.AddRange(configs.Where(x => removedRoles.Contains(x.RoleId) && x.Mode == RoleLinkingMode.GrantOnRevoke).Select(x => x.LinkedRoleId));

                rolesToRemove = configs.Where(x => addedRoles.Contains(x.RoleId) && x.Mode == RoleLinkingMode.RevokeOnGrant).Select(x => x.LinkedRoleId).ToList();
                rolesToRemove.AddRange(configs.Where(x => removedRoles.Contains(x.RoleId) && x.Mode == RoleLinkingMode.RevokeOnRevoke).Select(x => x.LinkedRoleId));

                rolesToAdd.RemoveAll(x =>
                {
                    var role = guild.GetRole(x);
                    return role is null || !role.CanBeManaged();
                });

                rolesToRemove.RemoveAll(x =>
                {
                    var role = guild.GetRole(x);
                    return role is null || !role.CanBeManaged();
                });

                _actions.AddRange(rolesToAdd.Select(x => new RoleLinkAction(guild.Id, e.NewMember.Id, x, RoleLinkActionType.Added)));
                _actions.AddRange(rolesToRemove.Select(x => new RoleLinkAction(guild.Id, e.NewMember.Id, x, RoleLinkActionType.Removed)));
            }

            foreach (var roleId in rolesToAdd)
            {
                await e.NewMember.GrantRoleAsync(roleId, new DefaultRestRequestOptions { Reason = "Role Linking" });
                await Task.Delay(1000);
            }
            foreach (var roleId in rolesToRemove)
            {
                await e.NewMember.RevokeRoleAsync(roleId, new DefaultRestRequestOptions { Reason = "Role Linking" });
                await Task.Delay(1000);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown on member updated");
        }
    }

    private class RoleLinkAction
    {
        public ulong GuildId { get; }
        public ulong UserId { get; }
        public ulong RoleId { get; }
        public RoleLinkActionType ActionType { get; }

        public RoleLinkAction(ulong guildId, ulong userId, ulong roleId, RoleLinkActionType actionType)
        {
            GuildId = guildId;
            UserId = userId;
            RoleId = roleId;
            ActionType = actionType;
        }
    }

    private enum RoleLinkActionType
    {
        Added,
        Removed
    }
}
