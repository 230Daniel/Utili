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
        ILogger<VoiceRolesService> _logger;
        DiscordClientBase _client;

        List<VoiceUpdateRequest> _updateRequests = new List<VoiceUpdateRequest>();

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

        async Task UpdateMembersAsync()
        {
            while (true)
            {
                try
                {
                    List<VoiceUpdateRequest> requests = new();

                    lock (_updateRequests)
                    {
                        foreach (VoiceUpdateRequest request in _updateRequests)
                        {
                            VoiceUpdateRequest existingRequest = requests.FirstOrDefault(x => x.GuildId == request.GuildId && x.MemberId == request.MemberId);
                            if (existingRequest is not null) existingRequest.NewChannelId = request.NewChannelId;
                            else requests.Add(request);
                        }

                        _updateRequests.Clear();
                    }

                    List<Task> tasks = requests.Select(UpdateMemberAsync).ToList();
                    await Task.WhenAll(tasks);

                    await Task.Delay(250);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Exception thrown starting member updates");
                }
            }
        }

        Task UpdateMemberAsync(VoiceUpdateRequest request)
        {
            return Task.Run(async () =>
            {
                try
                {
                    if(request.NewChannelId == request.OldChannelId) return;

                    IGuild guild = _client.GetGuild(request.GuildId);
                    IMember member = guild.GetMember(request.MemberId);

                    List<VoiceRolesRow> rows = await VoiceRoles.GetRowsAsync(request.GuildId);
                    rows.RemoveAll(x => guild.GetRole(x.RoleId) is null);

                    Snowflake? oldRoleId = null;
                    Snowflake? newRoleId = null;

                    if (request.OldChannelId.HasValue)
                    {
                        VoiceRolesRow row = rows.FirstOrDefault(x => x.ChannelId == request.OldChannelId.Value) ?? 
                                            rows.FirstOrDefault(x => x.ChannelId == 0);
                        oldRoleId = row?.RoleId;
                    }

                    if (request.NewChannelId.HasValue)
                    {
                        VoiceRolesRow row = rows.FirstOrDefault(x => x.ChannelId == request.NewChannelId.Value) ?? 
                                            rows.FirstOrDefault(x => x.ChannelId == 0);
                        newRoleId = row?.RoleId;
                    }

                    if(oldRoleId == newRoleId) return;

                    if (oldRoleId.HasValue && guild.GetRole(oldRoleId.Value).CanBeManaged())
                        await member.RevokeRoleAsync(oldRoleId.Value);

                    if (newRoleId.HasValue && guild.GetRole(newRoleId.Value).CanBeManaged())
                        await member.GrantRoleAsync(newRoleId.Value);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Exception thrown updating member");
                }
            });
        }

        class VoiceUpdateRequest
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
