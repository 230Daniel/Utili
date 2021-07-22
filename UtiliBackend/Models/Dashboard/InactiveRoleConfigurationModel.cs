namespace UtiliBackend.Models
{
    public class InactiveRoleConfigurationModel
    {
        public string RoleId { get; set; }
        public string ImmuneRoleId { get; set; }
        public string Threshold { get; set; }
        public int Mode { get; set; }
        public bool AutoKick { get; set; }
        public string AutoKickThreshold { get; set; }
    }
}
