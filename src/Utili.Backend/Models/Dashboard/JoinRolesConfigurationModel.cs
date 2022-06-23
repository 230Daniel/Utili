using System.Collections.Generic;
using System.Linq;
using Utili.Database.Entities;

namespace Utili.Backend.Models
{
    public class JoinRolesConfigurationModel
    {
        public bool WaitForVerification { get; set; }
        public List<string> JoinRoles { get; set; }
        public bool CancelOnRolePersist { get; set; }

        public void ApplyTo(JoinRolesConfiguration configuration)
        {
            configuration.WaitForVerification = WaitForVerification;
            configuration.JoinRoles = JoinRoles.Select(ulong.Parse).ToList();
            configuration.CancelOnRolePersist = CancelOnRolePersist;
        }
    }
}
