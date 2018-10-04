using System.Collections.Generic;
using TreeDiagram.Models.Server.Filter.IgnoreChannel;

namespace TreeDiagram.Models.Server.Filter
{
    public class FilterUrl : FilterBase<FilterUrlIgnoreChannel>
    {
        public bool BlockServerInvites { get; set; } = false;
        public bool DenyMode { get; set; } = false;

        public List<string> BannedUrls { get; private set; } = new List<string>();
        
        public FilterUrl(ulong id) : base(id) { }
    }
}