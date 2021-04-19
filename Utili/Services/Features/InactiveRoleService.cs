using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Disqord.Rest.Default;
using Microsoft.Extensions.Logging;
using Utili.Extensions;

namespace Utili.Services
{
    public class InactiveRoleService
    {
        ILogger<InactiveRoleService> _logger;
        DiscordClientBase _client;
        Timer _timer;

        public InactiveRoleService(ILogger<InactiveRoleService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;

            _timer = new Timer(30000);
            _timer.Elapsed += Timer_Elapsed;
        }

        public void Start()
        {
            _timer.Start();
        }

        public Task MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if(!e.GuildId.HasValue || e.Member.IsBot) return;
                    await MakeUserActiveAsync(e.GuildId.Value, e.Member);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception thrown on message received");
                }
            });
            return Task.CompletedTask;
        }

        public Task VoiceStateUpdated(object sender, VoiceStateUpdatedEventArgs e)
        {
            _ = Task.Run(async () =>
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
            });
            return Task.CompletedTask;
        }

        async Task MakeUserActiveAsync(ulong guildId, IMember member)
        {
            InactiveRoleRow row = await InactiveRole.GetRowAsync(guildId);
            IGuild guild = _client.GetGuild(guildId);
            IRole inactiveRole = guild.GetRole(row.RoleId);

            if(inactiveRole is null) return;

            await InactiveRole.UpdateUserAsync(guild.Id, member.Id);

            if(member.GetRole(inactiveRole.Id) is not null && inactiveRole.CanBeManaged())
                await member.RevokeRoleAsync(inactiveRole.Id);
        }

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = UpdateGuildsAsync();
        }

        async Task UpdateGuildsAsync()
        {
            try
            {
                List<InactiveRoleRow> guildsRequiringUpdate =
                    (await InactiveRole.GetUpdateRequiredRowsAsync())
                    .Where(x => _client.GetGuild(x.GuildId) is not null)
                    .Take(5).ToList();

                foreach (InactiveRoleRow row in guildsRequiringUpdate)
                {
                    row.LastUpdate = DateTime.UtcNow;
                    await InactiveRole.SaveLastUpdateAsync(row);
                }

                IEnumerable<Task> tasks = guildsRequiringUpdate.Select(UpdateGuildAsync);
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception thrown updating all guilds");
            }
        }

        Task UpdateGuildAsync(InactiveRoleRow row)
        {
            return Task.Run(async () =>
            {
                try
                {
                    IGuild guild = _client.GetGuild(row.GuildId);
                    IRole inactiveRole = guild.GetRole(row.RoleId);
                    if(inactiveRole is null || !inactiveRole.CanBeManaged()) return;

                    List<InactiveRoleUserRow> userRows = await InactiveRole.GetUsersAsync(guild.Id);
                    bool premium = await Premium.IsGuildPremiumAsync(guild.Id);

                    IReadOnlyList<IMember> members = await guild.FetchAllMembersAsync();
                    IMember bot = guild.GetCurrentMember();

                    foreach (IMember member in members.Where(x => !x.IsBot).OrderBy(x => x.Id))
                    {
                        // DefaultLastAction is set to the time when the activity data started being recorded
                        DateTime lastAction = row.DefaultLastAction;

                        // If the bot joined after activity data started being recorded, we know our data before the bot joined is invalid
                        if (bot.JoinedAt.HasValue && bot.JoinedAt.Value.UtcDateTime > lastAction)
                            lastAction = bot.JoinedAt.Value.UtcDateTime;

                        // If the member did something since the default last action, their last action is more recent
                        InactiveRoleUserRow userRow = userRows.FirstOrDefault(x => x.UserId == member.Id);
                        if (userRow is not null && userRow.LastAction > lastAction)
                            lastAction = userRow.LastAction;
                        
                        // If the member joined since the default (or their) last action, their last action is more recent
                        if (member.JoinedAt.HasValue && member.JoinedAt.Value.UtcDateTime > lastAction)
                            lastAction = member.JoinedAt.Value.UtcDateTime;

                        DateTime minimumLastAction = DateTime.UtcNow - row.Threshold;
                        DateTime minimumKickLastAction = DateTime.UtcNow - (row.Threshold + row.AutoKickThreshold);

                        if (lastAction <= minimumLastAction && !member.RoleIds.Contains(new Snowflake(row.ImmuneRoleId)))
                        {
                            if (premium && row.AutoKick && lastAction <= minimumKickLastAction)
                            {
                                if(guild.BotHasPermissions(Permission.KickMembers))
                                    await member.KickAsync(new DefaultRestRequestOptions(){Reason = "Kicked automatically for being inactive"});
                                await Task.Delay(500);
                                continue;
                            }

                            if (!row.Inverse)
                            {
                                if (!member.RoleIds.Contains(inactiveRole.Id))
                                {
                                    await member.GrantRoleAsync(inactiveRole.Id);
                                    await Task.Delay(500);
                                }
                            }
                            else
                            {
                                if (member.RoleIds.Contains(inactiveRole.Id))
                                {
                                    await member.RevokeRoleAsync(inactiveRole.Id);
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
                                    await member.RevokeRoleAsync(inactiveRole.Id);
                                    await Task.Delay(500);
                                }
                            }
                            else
                            {
                                if (!member.RoleIds.Contains(inactiveRole.Id))
                                {
                                    await member.GrantRoleAsync(inactiveRole.Id);
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
