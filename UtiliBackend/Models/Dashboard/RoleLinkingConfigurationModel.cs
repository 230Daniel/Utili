using NewDatabase.Entities;

namespace UtiliBackend.Models
{
    public class RoleLinkingConfigurationModel
    {
        public int Id { get; set; }
        public string RoleId { get; set; }
        public string LinkedRoleId { get; set; }
        public int Mode { get; set; }

        public void ApplyTo(RoleLinkingConfiguration configuration)
        {
            configuration.RoleId = ulong.Parse(RoleId);
            configuration.LinkedRoleId = ulong.Parse(LinkedRoleId);
            configuration.Mode = (RoleLinkingMode) Mode;
        }
    }
}
