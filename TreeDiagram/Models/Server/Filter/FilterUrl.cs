using System.Collections.Generic;

namespace TreeDiagram.Models.Server.Filter
{
    public class FilterUrl : FilterBase
    {
        public bool BlockServerInvites { get; set; }
        public bool DenyMode { get; set; }

        public List<string> BannedUrls { get; private set; } = new List<string>();
        
        public FilterUrl(ulong id) : base(id) { }
    }
}