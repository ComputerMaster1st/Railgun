using TreeDiagram.Models.Server.Filter.IgnoreChannel;

namespace TreeDiagram.Models.Server.Filter
{
    public class FilterCaps : FilterBase<FilterCapsIgnoreChannel>
    {
        public int Percentage { get; set; } = 75;
        public int Length { get; set; } = 0;
        
        public FilterCaps(ulong id) : base(id) { }
    }
}