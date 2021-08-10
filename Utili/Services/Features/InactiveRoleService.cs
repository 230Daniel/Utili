using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NewDatabase.Entities;
using NewDatabase.Extensions;
using Utili.Extensions;

namespace Utili.Services
{
    public class InactiveRoleService
    {
        private static readonly TimeSpan GapBetweenUpdates = TimeSpan.FromMinutes(60);
        
        private readonly ILogger<InactiveRoleService> _logger;
        private readonly DiscordClientBase _client;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly MemberCacheService _memberCache;

        private Timer _timer;

        public InactiveRoleService(ILogger<InactiveRoleService> logger, DiscordClientBase client, IServiceScopeFactory scopeFactory, MemberCacheService memberCache)
        {
            _logger = logger;
            _client = client;
            _scopeFactory = scopeFactory;
            _memberCache = memberCache;
            
            _timer = new Timer(30000);
            _timer.Elapsed += Timer_Elapsed;
        }

        public void Start()
        {
            _timer.Start();
        }

        public async Task MessageReceived(IServiceScope scope, MessageReceivedEventArgs e)
        {
            try
            {
                if(!e.GuildId.HasValue || e.Member is null || e.Member.IsBot) return;
                await MakeUserActiveAsync(scope, e.GuildId.Value, e.Member);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message received ({Guild}/{Channel}/{Message})", e.GuildId, e.ChannelId, e.MessageId);
            }
        }

        public async Task VoiceStateUpdated(IServiceScope scope, VoiceStateUpdatedEventArgs e)
        {
            try
            {
                if(e.Member.IsBot) return;
                await MakeUserActiveAsync(scope, e.GuildId, e.Member);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on voice state updated");
            }
        }

        private async Task MakeUserActiveAsync(IServiceScope scope, Snowflake guildId, IMember member)
        {
            var db = scope.GetDbContext();
            var config = await db.InactiveRoleConfigurations.GetForGuildAsync(guildId);
            IGuild guild = _client.GetGuild(guildId);
            var inactiveRole = guild.GetRole(config.RoleId);

            if(inactiveRole is null) return;

            var memberRecord = await db.InactiveRoleMembers.GetForMemberAsync(guildId, member.Id);
            if (memberRecord is null)
            {
                memberRecord = new InactiveRoleMember(guildId, member.Id)
                {
                    LastAction = DateTime.UtcNow
                };
                db.InactiveRoleMembers.Add(memberRecord);
                await db.SaveChangesAsync();
            }
            else
            {
                memberRecord.LastAction = DateTime.UtcNow;
                db.InactiveRoleMembers.Update(memberRecord);
                await db.SaveChangesAsync();
            }

            if (inactiveRole.CanBeManaged())
            {
                if (config.Mode == InactiveRoleMode.GrantWhenInactive && member.GetRole(inactiveRole.Id) is not null)
                    await member.RevokeRoleAsync(inactiveRole.Id, new DefaultRestRequestOptions {Reason = "Inactive Role"});
                else if (config.Mode == InactiveRoleMode.RevokeWhenInactive && member.GetRole(inactiveRole.Id) is null)
                    await member.GrantRoleAsync(inactiveRole.Id, new DefaultRestRequestOptions {Reason = "Inactive Role"});
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = UpdateGuildsAsync();
        }

        private async Task UpdateGuildsAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.GetDbContext();

                var now = DateTime.UtcNow;
                var maximumLastUpdate = now - GapBetweenUpdates;
                var configsRequiringUpdate = await db.InactiveRoleConfigurations.Where(x => x.LastUpdate < maximumLastUpdate).ToListAsync();
                configsRequiringUpdate.RemoveAll(x => _client.GetGuild(x.GuildId) is null);
                configsRequiringUpdate = configsRequiringUpdate.Take(5).ToList();

                foreach (var config in configsRequiringUpdate)
                {
                    config.LastUpdate = now;
                    db.InactiveRoleConfigurations.Update(config);
                }

                await db.SaveChangesAsync();

                var tasks = configsRequiringUpdate.Select(UpdateGuildAsync);
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception thrown updating all guilds");
            }
        }

        private Task UpdateGuildAsync(InactiveRoleConfiguration config)
        {
            return Task.Run(async () =>
            {
                try
                {
                    IGuild guild = _client.GetGuild(config.GuildId);
                    var inactiveRole = guild.GetRole(config.RoleId);
                    if(inactiveRole is null || !inactiveRole.CanBeManaged()) return;

                    await _memberCache.TemporarilyCacheMembersAsync(config.GuildId);
                    var bot = guild.GetCurrentMember();

                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.GetDbContext();

                    var userRows = await db.InactiveRoleMembers.GetForAllGuildMembersAsync(config.GuildId);
                    var premium = await db.GetIsGuildPremiumAsync(config.GuildId);

                    foreach (var member in guild.GetMembers().Values.Where(x => !x.IsBot).OrderBy(x => x.Id))
                    {
                        // DefaultLastAction is set to the time when the activity data started being recorded
                        var lastAction = config.DefaultLastAction;

                        // If the bot joined after activity data started being recorded, we know our data before the bot joined is invalid
                        if (bot.JoinedAt.HasValue && bot.JoinedAt.Value.UtcDateTime > lastAction)
                            lastAction = bot.JoinedAt.Value.UtcDateTime;

                        // If the member did something since the default last action, their last action is more recent
                        var userRow = userRows.FirstOrDefault(x => x.MemberId == member.Id);
                        if (userRow is not null && userRow.LastAction > lastAction)
                            lastAction = userRow.LastAction;
                        
                        // If the member joined since the default (or their) last action, their last action is more recent
                        if (member.JoinedAt.HasValue && member.JoinedAt.Value.UtcDateTime > lastAction)
                            lastAction = member.JoinedAt.Value.UtcDateTime;

                        var minimumLastAction = DateTime.UtcNow - config.Threshold;
                        var minimumKickLastAction = DateTime.UtcNow - (config.Threshold + config.AutoKickThreshold);

                        if (lastAction <= minimumLastAction && !member.RoleIds.Contains(config.ImmuneRoleId))
                        {
                            if (premium && config.AutoKick && lastAction <= minimumKickLastAction)
                            {
                                if(guild.BotHasPermissions(Permission.KickMembers) && member.CanBeManaged())
                                    await member.KickAsync(new DefaultRestRequestOptions {Reason = "Inactive Role (auto-kick)"});
                                await Task.Delay(500);
                                continue;
                            }

                            if (config.Mode == InactiveRoleMode.GrantWhenInactive)
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
                            if (config.Mode == InactiveRoleMode.GrantWhenInactive)
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
                    _logger.LogError(e, $"Exception thrown updating guild {config.GuildId}");
                }
            });
        }
    }
}
