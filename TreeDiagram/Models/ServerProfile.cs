using TreeDiagram.Models.Filter;

namespace TreeDiagram.Models
{
    public class ServerProfile
    {
        public virtual ServerFilters Filters { get; private set; } = null;
    }
}