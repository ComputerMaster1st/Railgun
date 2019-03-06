namespace TreeDiagram.Models.Server.Filter
{
    public class FilterCaps : FilterBase
    {
        public int Percentage { get; set; } = 75;
        public int Length { get; set; }
        
        public FilterCaps(ulong id) : base(id) { }
    }
}