namespace TreeDiagram.Models.Filter
{
    public class ServerFilters
    {
        public virtual FilterCaps Caps { get; set; } = null;
        public virtual FilterUrl Urls { get; set; } = null;
    }
}