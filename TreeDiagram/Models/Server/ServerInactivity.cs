using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TreeDiagram.Models.SubModels;

namespace TreeDiagram.Models.Server
{
    public class ServerInactivity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public bool IsEnabled { get; set; }
        public int InactiveDaysThreshold { get; set; }
        public int KickDaysThreshold { get; set; }
        public ulong InactiveRoleId { get; set; }

        public virtual List<UserActivityContainer> Users { get; private set; } = new List<UserActivityContainer>();

        public virtual List<ulong> UserWhitelist { get; private set; } = new List<ulong>();
        public virtual List<ulong> RoleWhitelist { get; private set; } = new List<ulong>();
    }
}
