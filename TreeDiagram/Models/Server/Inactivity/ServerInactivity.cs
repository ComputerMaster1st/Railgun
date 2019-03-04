using System.Collections.Generic;

namespace TreeDiagram.Models.Server.Inactivity
{
    public class ServerInactivity : ConfigBase
    {
        public bool IsEnabled { get; set; } = false;
        public bool SendInvite { get; set; } = false;

        public int InactiveThreshold { get; set; } = 0;
        public int KickThreshold { get; set; } = 0;

        public ulong InactiveRoleId { get; set; } = 0;

        public List<UserActivityContainer> Users { get; private set; } = new List<UserActivityContainer>();

        public List<ulong> UserWhitelist { get; private set; } = new List<ulong>();
        public List<ulong> RoleWhitelist { get; private set; } = new List<ulong>();

        public ServerInactivity(ulong id) : base(id) {}
    }
}
