using System.Collections.Generic;
using TreeDiagram.Models.SubModels;

namespace TreeDiagram.Interfaces
{
    public interface ITreeFilter
    {
        bool IsEnabled { get; set; }
        bool IncludeBots { get; set; }
        
        List<ulong> IgnoredChannels { get; }
    }
}