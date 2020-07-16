using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TreeDiagram.Models.Server
{
    public class ServerRoleRequest
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Column(TypeName="jsonb")]
        public virtual List<ulong> RoleIds { get; private set; } = new List<ulong>();

        public bool AddRole(ulong roleId)
        {
            if (RoleIds.Any(x => x == roleId)) return false;
            RoleIds.Add(roleId);
            return true;
        }

        public bool RemoveRole(ulong roleId)
        {
            if (RoleIds.RemoveAll(x => x == roleId) > 0) return true;
            return false;
        }
    }
}