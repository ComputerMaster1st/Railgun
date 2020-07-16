using TreeDiagram.Models.Filter;
using TreeDiagram.Models.Fun;
using TreeDiagram.Models.Server;

namespace TreeDiagram.Models
{
    public class ServerProfile
    {
        public virtual ServerFilters Filters { get; private set; } = new ServerFilters();
        public virtual ServerFun Fun { get; private set; } = new ServerFun();
        public virtual ServerCommand Command { get; set; } = null;
        public virtual ServerGlobals Globals { get; set; } = new ServerGlobals();
        public virtual ServerInactivity Inactivity { get; set; } = null;
        public virtual ServerJoinLeave JoinLeave { get; set; } = null;
    }
}