namespace TreeDiagram.Models.Filter
{
    public class FilterCaps : FilterBase
    {
        public int Percentage { get; set; } = 75;
        public int Length { get; set; }
        
        public FilterCaps(ulong id) : base(id) { }
    }
}