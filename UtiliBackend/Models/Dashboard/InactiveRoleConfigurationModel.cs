using System.Xml;
using Database.Entities;

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

        public void ApplyTo(InactiveRoleConfiguration configuration)
        {
            configuration.RoleId = ulong.Parse(RoleId);
            configuration.ImmuneRoleId = ulong.Parse(ImmuneRoleId);
            configuration.Threshold = XmlConvert.ToTimeSpan(Threshold);
            configuration.Mode = (InactiveRoleMode) Mode;
            configuration.AutoKick = AutoKick;
            configuration.AutoKickThreshold = XmlConvert.ToTimeSpan(AutoKickThreshold);
        }
    }
}
