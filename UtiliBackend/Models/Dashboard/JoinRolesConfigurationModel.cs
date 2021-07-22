using System.Collections.Generic;

namespace UtiliBackend.Models
{
    public class JoinRolesConfigurationModel
    {
        public bool WaitForVerification { get; set; }
        public List<string> JoinRoles { get; set; }
    }
}
