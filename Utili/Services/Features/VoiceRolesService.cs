using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using Utili.Extensions;

namespace Utili.Services
{
    public class VoiceRolesService
    {
        private readonly ILogger<VoiceRolesService> _logger;
        private readonly DiscordClientBase _client;

        private List<VoiceUpdateRequest> _updateRequests = new();

        public VoiceRolesService(ILogger<VoiceRolesService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;
        }

        public void Start()
        {
            _ = UpdateMembersAsync();
        }
        
        public async Task VoiceStateUpdated(VoiceStateUpdatedEventArgs e)
        {
            try
            {
                lock (_updateRequests)
                {
                    _updateRequests.Add(new VoiceUpdateRequest(e.GuildId, e.MemberId, e.OldVoiceState?.ChannelId, e.NewVoiceState?.ChannelId));
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in voice state updated");
            }
        }

        private async Task UpdateMembersAsync()
        {
            while (true)
            {
                try
                {
                    var requests = new List<VoiceUpdateRequest>();

                    lock (_updateRequests)
                    {
                        foreach (var request in _updateRequests)
                        {
                            var existingRequest = requests.FirstOrDefault(x => x.GuildId == request.GuildId && x.MemberId == request.MemberId);
                            if (existingRequest is not null) existingRequest.NewChannelId = request.NewChannelId;
                            else requests.Add(request);
                        }

                        _updateRequests.Clear();
                    }

                    var tasks = requests.Select(UpdateMemberAsync).ToList();
                    await Task.WhenAll(tasks);

                    await Task.Delay(250);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Exception thrown starting member updates");
                }
            }
        }

        private Task UpdateMemberAsync(VoiceUpdateRequest request)
        {
            return Task.Run(async () =>
            {
                try
                {
                    if(request.NewChannelId == request.OldChannelId) return;

                    IGuild guild = _client.GetGuild(request.GuildId);
                    IMember member = guild.GetMember(request.MemberId);

                    var rows = await VoiceRoles.GetRowsAsync(request.GuildId);
                    rows.RemoveAll(x => guild.GetRole(x.RoleId) is null);

                    Snowflake? oldRoleId = null;
                    Snowflake? newRoleId = null;

                    if (request.OldChannelId.HasValue)
                    {
                        var row = rows.FirstOrDefault(x => x.ChannelId == request.OldChannelId.Value) ?? 
                                  rows.FirstOrDefault(x => x.ChannelId == 0);
                        oldRoleId = row?.RoleId;
                    }

                    if (request.NewChannelId.HasValue)
                    {
                        var row = rows.FirstOrDefault(x => x.ChannelId == request.NewChannelId.Value) ?? 
                                  rows.FirstOrDefault(x => x.ChannelId == 0);
                        newRoleId = row?.RoleId;
                    }

                    if(oldRoleId == newRoleId) return;

                    if (oldRoleId.HasValue && guild.GetRole(oldRoleId.Value).CanBeManaged())
                        await member.RevokeRoleAsync(oldRoleId.Value, new DefaultRestRequestOptions{Reason = "Voice Roles"});

                    if (newRoleId.HasValue && guild.GetRole(newRoleId.Value).CanBeManaged())
                        await member.GrantRoleAsync(newRoleId.Value, new DefaultRestRequestOptions{Reason = "Voice Roles"});
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Exception thrown updating member");
                }
            });
        }

        private class VoiceUpdateRequest
        {
            public Snowflake GuildId { get; }
            public Snowflake MemberId { get; }
            public Snowflake? OldChannelId { get; }
            public Snowflake? NewChannelId { get; set; }

            public VoiceUpdateRequest(Snowflake guildId, Snowflake memberId, Snowflake? oldChannelId, Snowflake? newChannelId)
            {
                GuildId = guildId;
                MemberId = memberId;
                OldChannelId = oldChannelId;
                NewChannelId = newChannelId;
            }
        }
    }

    
}
