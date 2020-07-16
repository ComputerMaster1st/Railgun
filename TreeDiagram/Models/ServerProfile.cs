using TreeDiagram.Models.Filter;
using TreeDiagram.Models.Fun;

namespace TreeDiagram.Models
{
    public class ServerProfile
    {
        public virtual ServerFilters Filters { get; private set; } = new ServerFilters();
        public virtual ServerFun Fun { get; private set; } = new ServerFun();
    }
}