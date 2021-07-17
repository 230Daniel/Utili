﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using Utili.Extensions;

namespace Utili.Services
{
    public class InactiveRoleService
    {
        private readonly ILogger<InactiveRoleService> _logger;
        private readonly DiscordClientBase _client;
        private readonly MemberCacheService _memberCache;
        
        private Timer _timer;
        
        public InactiveRoleService(ILogger<InactiveRoleService> logger, DiscordClientBase client, MemberCacheService memberCache)
        {
            _logger = logger;
            _client = client;
            _memberCache = memberCache;
            
            _timer = new Timer(30000);
            _timer.Elapsed += Timer_Elapsed;
        }

        public void Start()
        {
            _timer.Start();
        }

        public async Task MessageReceived(MessageReceivedEventArgs e)
        {
            try
            {
                if(!e.GuildId.HasValue || e.Member is null || e.Member.IsBot) return;
                await MakeUserActiveAsync(e.GuildId.Value, e.Member);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message received ({Guild}/{Channel}/{Message})", e.GuildId, e.ChannelId, e.MessageId);
            }
        }

        public async Task VoiceStateUpdated(VoiceStateUpdatedEventArgs e)
        {
            try
            {
                if(e.Member.IsBot) return;
                await MakeUserActiveAsync(e.GuildId, e.Member);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on voice state updated");
            }
        }

        private async Task MakeUserActiveAsync(ulong guildId, IMember member)
        {
            var row = await InactiveRole.GetRowAsync(guildId);
            IGuild guild = _client.GetGuild(guildId);
            var inactiveRole = guild.GetRole(row.RoleId);

            if(inactiveRole is null) return;

            await InactiveRole.UpdateUserAsync(guild.Id, member.Id);

            if(member.GetRole(inactiveRole.Id) is not null && inactiveRole.CanBeManaged())
                await member.RevokeRoleAsync(inactiveRole.Id, new DefaultRestRequestOptions {Reason = "Inactive Role"});
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = UpdateGuildsAsync();
        }

        private async Task UpdateGuildsAsync()
        {
            try
            {
                var guildsRequiringUpdate =
                    (await InactiveRole.GetUpdateRequiredRowsAsync())
                    .Where(x => _client.GetGuild(x.GuildId) is not null)
                    .Take(5).ToList();

                foreach (var row in guildsRequiringUpdate)
                {
                    row.LastUpdate = DateTime.UtcNow;
                    await InactiveRole.SaveLastUpdateAsync(row);
                }

                var tasks = guildsRequiringUpdate.Select(UpdateGuildAsync);
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception thrown updating all guilds");
            }
        }

        private Task UpdateGuildAsync(InactiveRoleRow row)
        {
            return Task.Run(async () =>
            {
                try
                {
                    IGuild guild = _client.GetGuild(row.GuildId);
                    var inactiveRole = guild.GetRole(row.RoleId);
                    if(inactiveRole is null || !inactiveRole.CanBeManaged()) return;

                    await _memberCache.TemporarilyCacheMembersAsync(row.GuildId);
                    var bot = guild.GetCurrentMember();
                    
                    var userRows = await InactiveRole.GetUsersAsync(guild.Id);
                    var premium = await Premium.IsGuildPremiumAsync(guild.Id);

                    foreach (var member in guild.GetMembers().Values.Where(x => !x.IsBot).OrderBy(x => x.Id))
                    {
                        // DefaultLastAction is set to the time when the activity data started being recorded
                        var lastAction = row.DefaultLastAction;

                        // If the bot joined after activity data started being recorded, we know our data before the bot joined is invalid
                        if (bot.JoinedAt.HasValue && bot.JoinedAt.Value.UtcDateTime > lastAction)
                            lastAction = bot.JoinedAt.Value.UtcDateTime;

                        // If the member did something since the default last action, their last action is more recent
                        var userRow = userRows.FirstOrDefault(x => x.UserId == member.Id);
                        if (userRow is not null && userRow.LastAction > lastAction)
                            lastAction = userRow.LastAction;
                        
                        // If the member joined since the default (or their) last action, their last action is more recent
                        if (member.JoinedAt.HasValue && member.JoinedAt.Value.UtcDateTime > lastAction)
                            lastAction = member.JoinedAt.Value.UtcDateTime;

                        var minimumLastAction = DateTime.UtcNow - row.Threshold;
                        var minimumKickLastAction = DateTime.UtcNow - (row.Threshold + row.AutoKickThreshold);

                        if (lastAction <= minimumLastAction && !member.RoleIds.Contains(row.ImmuneRoleId))
                        {
                            if (premium && row.AutoKick && lastAction <= minimumKickLastAction)
                            {
                                if(guild.BotHasPermissions(Permission.KickMembers) && member.CanBeManaged())
                                    await member.KickAsync(new DefaultRestRequestOptions {Reason = "Inactive Role (auto-kick)"});
                                await Task.Delay(500);
                                continue;
                            }

                            if (!row.Inverse)
                            {
                                if (!member.RoleIds.Contains(inactiveRole.Id))
                                {
                                    await member.GrantRoleAsync(inactiveRole.Id, new DefaultRestRequestOptions {Reason = "Inactive Role"});
                                    await Task.Delay(500);
                                }
                            }
                            else
                            {
                                if (member.RoleIds.Contains(inactiveRole.Id))
                                {
                                    await member.RevokeRoleAsync(inactiveRole.Id, new DefaultRestRequestOptions {Reason = "Inactive Role"});
                                    await Task.Delay(500);
                                }
                            }
                        }
                        else
                        {
                            if (!row.Inverse)
                            {
                                if (member.RoleIds.Contains(inactiveRole.Id))
                                {
                                    await member.RevokeRoleAsync(inactiveRole.Id, new DefaultRestRequestOptions {Reason = "Inactive Role"});
                                    await Task.Delay(500);
                                }
                            }
                            else
                            {
                                if (!member.RoleIds.Contains(inactiveRole.Id))
                                {
                                    await member.GrantRoleAsync(inactiveRole.Id, new DefaultRestRequestOptions {Reason = "Inactive Role"});
                                    await Task.Delay(500);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Exception thrown updating guild {row.GuildId}");
                }
            });
        }
    }
}
