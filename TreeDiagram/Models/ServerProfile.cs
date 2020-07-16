using TreeDiagram.Models.Filter;
using TreeDiagram.Models.Fun;
using TreeDiagram.Models.Server;

namespace TreeDiagram.Models
{
    public class ServerProfile : IdBase
    {
        public virtual ServerFilters Filters { get; private set; } = new ServerFilters();
        public virtual ServerFun Fun { get; private set; } = new ServerFun();
        public virtual ServerCommand Command { get; set; } = null;
        public virtual ServerGlobals Globals { get; private set; } = new ServerGlobals();
        public virtual ServerInactivity Inactivity { get; set; } = null;
        public virtual ServerJoinLeave JoinLeave { get; set; } = null;
        public virtual ServerMusic Music { get; set; } = null;
        public virtual ServerRoleRequest RoleRequest { get; set; } = null;
        public virtual ServerWarning Warning { get; set; } = null;
        
        public ServerProfile(ulong id) : base(id) {}
    }
}