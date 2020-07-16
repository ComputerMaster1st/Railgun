using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TreeDiagram.Models.SubModels;

namespace TreeDiagram.Models.Server
{
    public class ServerInactivity
    {
        public bool IsEnabled { get; set; }
        public int InactiveDaysThreshold { get; set; }
        public int KickDaysThreshold { get; set; }
        public ulong InactiveRoleId { get; set; }

        public virtual List<UserActivityContainer> Users { get; private set; } = new List<UserActivityContainer>();

        [Column(TypeName="jsonb")]
        public virtual List<ulong> UserWhitelist { get; private set; } = new List<ulong>();
        [Column(TypeName="jsonb")]
        public virtual List<ulong> RoleWhitelist { get; private set; } = new List<ulong>();
    }
}
