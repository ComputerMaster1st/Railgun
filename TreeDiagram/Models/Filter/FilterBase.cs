using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TreeDiagram.Interfaces;

namespace TreeDiagram.Models.Filter
{
    public abstract class FilterBase : ITreeFilter
    {
        public bool IsEnabled { get; set; }
        public bool IncludeBots { get; set; }
        
        [Column(TypeName="jsonb")]
        public virtual List<ulong> IgnoredChannels { get; private set; } = new List<ulong>();
    }
}