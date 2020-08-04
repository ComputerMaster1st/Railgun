using TreeDiagram.Models.Filter;
using TreeDiagram.Models.Fun;
using TreeDiagram.Models.Server;

namespace TreeDiagram.Models
{
    public class ServerProfile : IdBase
    {
        public virtual ServerFilters Filters { get; private set; } = new ServerFilters();
        public virtual ServerFun Fun { get; private set; } = new ServerFun();
        public virtual ServerCommand Command { get; private set; } = new ServerCommand();
        public virtual ServerGlobals Globals { get; private set; } = new ServerGlobals();
        public virtual ServerInactivity Inactivity { get; private set; } = new ServerInactivity();
        public virtual ServerJoinLeave JoinLeave { get; private set; } = new ServerJoinLeave();
        public virtual ServerMusic Music { get; private set; } = new ServerMusic();
        public virtual ServerRoleRequest RoleRequest { get; private set; } = new ServerRoleRequest();
        public virtual ServerWarning Warning { get; private set; } = new ServerWarning();
        
        public ServerProfile(ulong id) : base(id) {}

        public void ResetCommands() {
            Command = null;
            Command = new ServerCommand();
        }

        public void ResetGlobals() {
            Globals = null;
            Globals = new ServerGlobals();
        }

        public void ResetInactivity() {
            Inactivity = null;
            Inactivity = new ServerInactivity();
        }

        public void ResetJoinLeave() {
            JoinLeave = null;
            JoinLeave = new ServerJoinLeave();
        }

        public void ResetMusic() {
            Music = null;
            Music = new ServerMusic();
        }

        public void ResetRoleRequest() {
            RoleRequest = null;
            RoleRequest = new ServerRoleRequest();
        }

        public void ResetWarning() {
            Warning = null;
            Warning = new ServerWarning();
        }
    }
}