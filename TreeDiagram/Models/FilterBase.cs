using System.Collections.Generic;
using TreeDiagram.Interfaces;
using TreeDiagram.Models.SubModels;

namespace TreeDiagram.Models
{
    public abstract class FilterBase : ConfigBase, ITreeFilter
    {
        public bool IsEnabled { get; set; }
        public bool IncludeBots { get; set; }
        
        public virtual List<IgnoredChannels> IgnoredChannels { get; private set; } = new List<IgnoredChannels>();

        protected FilterBase(ulong id) : base(id) { }
    }
}