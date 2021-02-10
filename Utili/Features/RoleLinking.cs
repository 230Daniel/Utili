﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database.Data;
using Discord.WebSocket;

namespace Utili.Features
{
    internal static class RoleLinking
    {
        private static List<RoleLinkAction> _actions = new List<RoleLinkAction>();

        public static async Task GuildUserUpdated(SocketGuildUser before, SocketGuildUser after)
        {
            SocketGuild guild = after.Guild;
            bool premium = await Premium.IsGuildPremiumAsync(guild.Id);
            List<RoleLinkingRow> rows = await Database.Data.RoleLinking.GetRowsAsync(guild.Id);
            if (!premium) rows = rows.Take(2).ToList();

            List<ulong> addedRoles = after.Roles.Where(x => before.Roles.All(y => y.Id != x.Id)).Select(x => x.Id).ToList();
            List<ulong> removedRoles = before.Roles.Where(x => after.Roles.All(y => y.Id != x.Id)).Select(x => x.Id).ToList();

            List<ulong> rolesToAdd = rows.Where(x => addedRoles.Contains(x.RoleId) && x.Mode == 0).Select(x => x.LinkedRoleId).ToList();
            rolesToAdd.AddRange(rows.Where(x => removedRoles.Contains(x.RoleId) && x.Mode == 2).Select(x => x.LinkedRoleId));

            List<ulong> rolesToRemove = rows.Where(x => addedRoles.Contains(x.RoleId) && x.Mode == 1).Select(x => x.LinkedRoleId).ToList();
            rolesToRemove.AddRange(rows.Where(x => removedRoles.Contains(x.RoleId) && x.Mode == 3).Select(x => x.LinkedRoleId));

            lock (_actions)
            {
                foreach (RoleLinkAction action in _actions.Where(x => x.GuildId == guild.Id && x.UserId == after.Id))
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

                _actions.AddRange(rolesToAdd.Select(x => new RoleLinkAction(guild.Id, after.Id, x, RoleLinkActionType.Added)));
                _actions.AddRange(rolesToRemove.Select(x => new RoleLinkAction(guild.Id, after.Id, x, RoleLinkActionType.Removed)));
            }

            foreach (ulong role in rolesToAdd)
            {
                try
                {
                    await after.AddRoleAsync(guild.GetRole(role));
                    await Task.Delay(1000);
                }
                catch { }
            }
            foreach (ulong role in rolesToRemove)
            {
                try
                {
                    await after.RemoveRoleAsync(guild.GetRole(role));
                    await Task.Delay(1000);
                }
                catch { }
            }
        }
    }

    internal class RoleLinkAction
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

    internal enum RoleLinkActionType
    {
        Added, 
        Removed
    }
}