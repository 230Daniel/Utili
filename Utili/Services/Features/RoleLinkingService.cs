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
    public class RoleLinkingService
    {
        ILogger<RoleLinkingService> _logger;
        DiscordClientBase _client;

        List<RoleLinkAction> _actions = new List<RoleLinkAction>();

        public RoleLinkingService(ILogger<RoleLinkingService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;
        }

        public Task MemberUpdated(object sender, MemberUpdatedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    IGuild guild = _client.GetGuild(e.NewMember.GuildId);
                    List<RoleLinkingRow> rows = await RoleLinking.GetRowsAsync(guild.Id);
                    bool premium = await Premium.IsGuildPremiumAsync(guild.Id);
                    if (!premium) rows = rows.Take(2).ToList();

                    List<CachedRole> oldRoles = e.OldMember.GetRoles().Values.ToList();
                    List<CachedRole> newRoles = e.NewMember.GetRoles().Values.ToList();

                    List<ulong> addedRoles = newRoles.Where(x => oldRoles.All(y => y.Id != x.Id)).Select(x => x.Id.RawValue).ToList();
                    List<ulong> removedRoles = oldRoles.Where(x => newRoles.All(y => y.Id != x.Id)).Select(x => x.Id.RawValue).ToList();

                    List<ulong> rolesToAdd;
                    List<ulong> rolesToRemove; 
                    
                    lock (_actions)
                    {
                        List<RoleLinkAction> actionsPerformedByBot = _actions.Where(x => x.GuildId == guild.Id && x.UserId == e.NewMember.Id).ToList();
                        foreach (RoleLinkAction action in actionsPerformedByBot)
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

                        rolesToAdd = rows.Where(x => addedRoles.Contains(x.RoleId) && x.Mode == 0).Select(x => x.LinkedRoleId).ToList();
                        rolesToAdd.AddRange(rows.Where(x => removedRoles.Contains(x.RoleId) && x.Mode == 2).Select(x => x.LinkedRoleId));

                        rolesToRemove = rows.Where(x => addedRoles.Contains(x.RoleId) && x.Mode == 1).Select(x => x.LinkedRoleId).ToList();
                        rolesToRemove.AddRange(rows.Where(x => removedRoles.Contains(x.RoleId) && x.Mode == 3).Select(x => x.LinkedRoleId));

                        _actions.AddRange(rolesToAdd.Select(x => new RoleLinkAction(guild.Id, e.NewMember.Id, x, RoleLinkActionType.Added)));
                        _actions.AddRange(rolesToRemove.Select(x => new RoleLinkAction(guild.Id, e.NewMember.Id, x, RoleLinkActionType.Removed)));
                    }

                    foreach (ulong roleId in rolesToAdd)
                    {
                        IRole role = guild.GetRole(roleId);
                        if (role.CanBeManaged())
                        {
                            await e.NewMember.GrantRoleAsync(roleId);
                            await Task.Delay(1000);
                        }
                    }
                    foreach (ulong roleId in rolesToRemove)
                    {
                        IRole role = guild.GetRole(roleId);
                        if (role.CanBeManaged())
                        {
                            await e.NewMember.RevokeRoleAsync(roleId);
                            await Task.Delay(1000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception thrown on member updated");
                }
            });
            return Task.CompletedTask;
        }

        class RoleLinkAction
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

        enum RoleLinkActionType
        {
            Added, 
            Removed
        }
    }
}
