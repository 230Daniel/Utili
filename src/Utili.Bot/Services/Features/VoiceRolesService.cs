using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Utili.Bot.Extensions;
using Utili.Database.Entities;

namespace Utili.Bot.Services
{
    public class VoiceRolesService
    {
        private readonly ILogger<VoiceRolesService> _logger;
        private readonly DiscordClientBase _client;
        private readonly IServiceScopeFactory _scopeFactory;

        private SemaphoreSlim _semaphore = new(1, 1);
        private List<UpdateRequest> _updateRequests = new();

        public VoiceRolesService(ILogger<VoiceRolesService> logger, DiscordClientBase client, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _client = client;
            _scopeFactory = scopeFactory;
        }

        public void Start()
        {
            _ = ContinuouslyActionAllUpdateRequestsAsync();
        }

        public async Task VoiceStateUpdated(VoiceStateUpdatedEventArgs e)
        {
            await _semaphore.WaitAsync();

            try
            {
                var existingRequest = _updateRequests.FirstOrDefault(x => x.GuildId == e.GuildId && x.MemberId == e.MemberId);

                if (existingRequest is not null)
                {
                    existingRequest.NewChannelId = e.NewVoiceState.ChannelId;
                    return;
                }

                var newRequest = new UpdateRequest(e.GuildId, e.MemberId, e.OldVoiceState?.ChannelId, e.NewVoiceState?.ChannelId);
                _updateRequests.Add(newRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on voice state updated");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task ContinuouslyActionAllUpdateRequestsAsync()
        {
            while (true)
            {
                var actionTask = ActionAllUpdateRequestsAsync();
                var delay = Task.Delay(1000);

                await Task.WhenAll(actionTask, delay);
            }
        }

        private async Task ActionAllUpdateRequestsAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.GetDbContext();
                var configs = await db.VoiceRoleConfigurations.ToListAsync();

                var tasks = new List<Task>();

                foreach (var updateRequest in _updateRequests)
                {
                    var guildConfigs = configs.Where(x => x.GuildId == updateRequest.GuildId);
                    var task = ActionUpdateRequestAsync(updateRequest, guildConfigs);
                    tasks.Add(task);
                }

                _updateRequests.Clear();

                var allTasks = Task.WhenAll(tasks);
                var delay = Task.Delay(5000);

                await Task.WhenAny(allTasks, delay);

                if (!allTasks.IsCompleted)
                {
                    var incompleteTaskCount = tasks.Count(x => !x.IsCompleted);
                    _logger.LogWarning("The five second maximum wait for actioning update requests was exceeded for {Count} update requests", incompleteTaskCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while actioning all update requests");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task ActionUpdateRequestAsync(UpdateRequest request, IEnumerable<VoiceRoleConfiguration> guildConfigurations)
        {
            await Task.Yield();

            try
            {
                if (request.OldChannelId == request.NewChannelId) return;

                var configurationForOldChannel = request.OldChannelId.HasValue ?
                    guildConfigurations.FirstOrDefault(x => x.ChannelId == request.OldChannelId) ??
                    guildConfigurations.FirstOrDefault(x => x.ChannelId == 0)
                    : null;

                var configurationForNewChannel = request.NewChannelId.HasValue ?
                    guildConfigurations.FirstOrDefault(x => x.ChannelId == request.NewChannelId) ??
                    guildConfigurations.FirstOrDefault(x => x.ChannelId == 0)
                    : null;

                if (configurationForOldChannel?.RoleId == configurationForNewChannel?.RoleId) return;

                var guild = _client.GetGuild(request.GuildId);

                if (configurationForOldChannel is not null)
                {
                    var role = guild.GetRole(configurationForOldChannel.RoleId);

                    if (role is not null && role.CanBeManaged())
                        await guild.RevokeRoleAsync(request.MemberId, role.Id, new DefaultRestRequestOptions { Reason = "Voice Roles" });
                }

                if (configurationForNewChannel is not null)
                {
                    var role = guild.GetRole(configurationForNewChannel.RoleId);

                    if (role is not null && role.CanBeManaged())
                        await guild.GrantRoleAsync(request.MemberId, role.Id, new DefaultRestRequestOptions { Reason = "Voice Roles" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while updating voice roles for member {GuildId}/{MemberId}, old channel {OldChannelId}, new channel {NewChannelId}",
                    request.GuildId, request.MemberId, request.OldChannelId, request.NewChannelId);
            }
        }

        private class UpdateRequest
        {
            public Snowflake GuildId { get; }
            public Snowflake MemberId { get; }
            public Snowflake? OldChannelId { get; }
            public Snowflake? NewChannelId { get; set; }

            public UpdateRequest(Snowflake guildId, Snowflake memberId, Snowflake? oldChannelId, Snowflake? newChannelId)
            {
                GuildId = guildId;
                MemberId = memberId;
                OldChannelId = oldChannelId;
                NewChannelId = newChannelId;
            }
        }
    }
}
