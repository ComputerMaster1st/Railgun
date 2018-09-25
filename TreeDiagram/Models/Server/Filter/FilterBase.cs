using System.Collections.Generic;

namespace TreeDiagram.Models.Server.Filter
{
    public abstract class FilterBase : ConfigBase
    {
        public bool IsEnabled { get; set; } = false;
        public bool IncludeBots { get; set; } = false;

        public List<ulong> IgnoredChannels { get; private set; } = new List<ulong>();

        protected FilterBase(ulong id) : base(id) { }
    }
}