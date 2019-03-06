using System.Collections.Generic;
using TreeDiagram.Models.SubModels;

namespace TreeDiagram.Models.Server
{
    public class ServerInactivity : ConfigBase
    {
        public bool IsEnabled { get; set; }
        public int InactiveDaysThreshold { get; set; }
        public int KickDaysThreshold { get; set; }
        public ulong InactiveRoleId { get; set; }

        public virtual List<UserActivityContainer> Users { get; private set; } = new List<UserActivityContainer>();

        public virtual List<UlongUserId> UserWhitelist { get; private set; } = new List<UlongUserId>();
        public virtual List<UlongRoleId> RoleWhitelist { get; private set; } = new List<UlongRoleId>();

        public ServerInactivity(ulong id) : base(id) {}
    }
}
