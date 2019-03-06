using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models.Server
{
    public class UlongRoleId {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; } = 0;
        public ulong RoleId { get; private set; }

        public UlongRoleId(ulong roleId) => RoleId = roleId;
    }
}