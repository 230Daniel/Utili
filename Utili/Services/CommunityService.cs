using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Utili.Extensions;

namespace Utili
{
    public class CommunityService
    {
        private readonly ILogger<CommunityService> _logger;
        private readonly IConfiguration _config;
        private readonly DiscordClientBase _client;
        private readonly Snowflake _communityGuildId;
        
        private Timer _roleTimer;

        public CommunityService(
            ILogger<CommunityService> logger, 
            IConfiguration config, 
            DiscordClientBase client)
        {
            _logger = logger;
            _config = config;
            _client = client;

            _communityGuildId = _config.GetSection("Community").GetValue<ulong>("GuildId");
        }

        public async Task GuildAvailable(GuildAvailableEventArgs e)
        {
            try
            {
                if (e.GuildId == _communityGuildId)
                {
                    _roleTimer?.Dispose();
                    await _client.Chunker.ChunkAsync(e.Guild);
                    _logger.LogDebug($"Finished chunking members for community guild ({e.Guild.Name})");
                    
                    _roleTimer = new Timer(60000);
                    _roleTimer.Elapsed += RoleTimer_Elapsed;
                    _roleTimer.Start();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on guild available");
            }
        }

        private void RoleTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var guild = _client.GetGuild(_communityGuildId);
                    var premiumRole = guild.GetRole(_config.GetSection("Community").GetValue<ulong>("PremiumRoleId"));
                    
                    if (premiumRole is not null)
                    {
                        var subscriptionRows = await Subscriptions.GetRowsAsync(onlyValid: true);
                        var premiumMembers = guild.Members.Select(x => x.Value).Where(x => subscriptionRows.Any(y => y.UserId == x.Id)).ToList();

                        foreach (var premiumMember in premiumMembers)
                            if (!premiumMember.RoleIds.Contains(premiumRole.Id))
                                await premiumMember.GrantRoleAsync(premiumRole.Id, new DefaultRestRequestOptions {Reason = "Premium"});

                        var markedPremiumMembers = guild.Members.Select(x => x.Value).Where(x => x.RoleIds.Contains(premiumRole.Id)).ToList();
                        
                        foreach (var premiumMember in markedPremiumMembers)
                            if (premiumMembers.All(x => x.Id != premiumMember.Id))
                                await premiumMember.RevokeRoleAsync(premiumRole.Id, new DefaultRestRequestOptions {Reason = "Premium"});
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception thrown on role timer elapsed");
                }
            });
        }
    }
}
