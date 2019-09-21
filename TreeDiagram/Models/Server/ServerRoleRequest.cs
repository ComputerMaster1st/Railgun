using System.Collections.Generic;
using System.Linq;
using TreeDiagram.Models.SubModels;

namespace TreeDiagram.Models.Server
{
    public class ServerRoleRequest : ConfigBase
    {
        public virtual List<UlongRoleId> RoleIds { get; private set; } = new List<UlongRoleId>();

        public ServerRoleRequest(ulong id) : base(id) { }

        public bool AddRole(ulong roleId)
        {
            if (RoleIds.Any(x => x.RoleId == roleId)) return false;
            RoleIds.Add(new UlongRoleId(roleId));
            return true;
        }

        public bool RemoveRole(ulong roleId)
        {
            if (RoleIds.RemoveAll(x => x.RoleId == roleId) > 0) return true;
            return false;
        }
    }
}