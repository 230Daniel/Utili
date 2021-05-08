using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Microsoft.Extensions.Logging;
using Utili.Extensions;

namespace Utili.Services
{
    public class RoleCacheService
    {
        ILogger<RoleCacheService> _logger;
        DiscordClientBase _client;

        public RoleCacheService(ILogger<RoleCacheService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task Ready(ReadyEventArgs e)
        {
            try
            {
                List<Task> tasks = new();
                foreach (Snowflake guildId in e.GuildIds)
                {
                    while (tasks.Count(x => !x.IsCompleted) >= 10)
                        await Task.Delay(100);
                    tasks.Add(CacheMembersAsync(guildId));
                }

                await Task.WhenAll(tasks);
                _logger.LogInformation("All members cached for {ShardId}", e.ShardId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on ready for {ShardId}", e.ShardId);
            }
        }

        public async Task MemberUpdated(MemberUpdatedEventArgs e)
        {
            try
            {
                RoleCacheRow row = await RoleCache.GetRowAsync(e.NewMember.GuildId, e.NewMember.Id);
                if (row.RoleIds.All(x => e.NewMember.RoleIds.Contains(x)) && 
                    e.NewMember.RoleIds.All(x => row.RoleIds.Contains(x))) return;

                row.RoleIds = e.NewMember.RoleIds.Select(x => x.RawValue).ToList();
                await row.SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on member updated");
            }
        }
        
        public async Task MemberLeft(MemberLeftEventArgs e)
        {
            try
            {
                RoleCacheRow row = await RoleCache.GetRowAsync(e.GuildId, e.User.Id);
                if (row.New) return;

                await row.DeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on member left");
            }
        }

        Task CacheMembersAsync(Snowflake guildId)
        {
            return Task.Run(async () =>
            {
                _logger.LogTrace("Started member fetching for {GuildId}", guildId);
                
                IGuild guild = _client.GetGuild(guildId);
                IReadOnlyList<IMember> members = await guild.FetchAllMembersAsync();
                List<RoleCacheRow> rows = await RoleCache.GetRowsAsync(guildId);
                
                _logger.LogTrace("Started database saving for {GuildId}", guildId);
                
                foreach (IMember member in members)
                {
                    RoleCacheRow row = rows.FirstOrDefault(x => x.UserId == member.Id);
                    row ??= new RoleCacheRow(guildId, member.Id);
                    row.RoleIds = member.RoleIds.Select(x => x.RawValue).ToList();
                    if(row.RoleIds.Any()) await row.SaveAsync();
                }
                
                _logger.LogDebug("All members cached for {GuildId}", guildId);
            });
        }
    }
}
