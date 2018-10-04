using System.Collections.Generic;

namespace TreeDiagram.Models.Server.Filter
{
    public abstract class FilterBase<T> : ConfigBase where T : class 
    {
        public bool IsEnabled { get; set; } = false;
        public bool IncludeBots { get; set; } = false;

        public List<T> IgnoredChannels { get; private set; } = new List<T>();

        protected FilterBase(ulong id) : base(id) { }
    }
}