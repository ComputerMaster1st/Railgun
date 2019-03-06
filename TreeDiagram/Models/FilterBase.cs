using System.Collections.Generic;
using TreeDiagram.Models.SubModels;

namespace TreeDiagram.Models
{
    public abstract class FilterBase : ConfigBase
    {
        public bool IsEnabled { get; set; }
        public bool IncludeBots { get; set; }
        
        public virtual List<IgnoredChannels> IgnoredChannels { get; private set; } = new List<IgnoredChannels>();

        protected FilterBase(ulong id) : base(id) { }
    }
}