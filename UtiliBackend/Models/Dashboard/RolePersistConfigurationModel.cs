using System.Collections.Generic;
using System.Linq;
using Database.Entities;

namespace UtiliBackend.Models
{
    public class RolePersistConfigurationModel
    {
        public bool Enabled { get; set; }
        public List<string> ExcludedRoles { get; set; }

        public void ApplyTo(RolePersistConfiguration configuration)
        {
            configuration.Enabled = Enabled;
            configuration.ExcludedRoles = ExcludedRoles.Select(ulong.Parse).ToList();
        }
    }
}
