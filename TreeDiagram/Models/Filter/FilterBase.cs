using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TreeDiagram.Interfaces;

namespace TreeDiagram.Models.Filter
{
    public abstract class FilterBase : ITreeFilter
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public bool IsEnabled { get; set; }
        public bool IncludeBots { get; set; }
        
        public virtual List<ulong> IgnoredChannels { get; private set; } = new List<ulong>();
    }
}