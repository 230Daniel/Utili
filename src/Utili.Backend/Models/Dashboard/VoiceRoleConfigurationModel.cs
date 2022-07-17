using Utili.Database.Entities;

namespace Utili.Backend.Models;

public class VoiceRoleConfigurationModel
{
    public string ChannelId { get; set; }
    public string RoleId { get; set; }

    public void ApplyTo(VoiceRoleConfiguration configuration)
    {
        configuration.RoleId = ulong.Parse(RoleId);
    }
}