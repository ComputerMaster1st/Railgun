using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TreeDiagram.Models.Server
{
    public class ServerRoleRequest
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        private List<ulong> _roleIds;
        
        public List<ulong> RoleIds { 
            get {
                if (_roleIds == null) _roleIds = new List<ulong>();
                return _roleIds;
            } private set {
                _roleIds = value;
            }}

        public bool AddRole(ulong roleId)
        {
            if (RoleIds.Contains(roleId)) return false;

            RoleIds = new List<ulong>(RoleIds);
            RoleIds.Add(roleId);
            return true;
        }

        public bool RemoveRole(ulong roleId)
        {
            if (!RoleIds.Contains(roleId)) return false;

            RoleIds = new List<ulong>(RoleIds);
            RoleIds.RemoveAll(x => x == roleId);
            return true;            
        }
    }
}