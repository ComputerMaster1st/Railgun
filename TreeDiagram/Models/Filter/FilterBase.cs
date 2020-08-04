using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TreeDiagram.Interfaces;

namespace TreeDiagram.Models.Filter
{
    public abstract class FilterBase : ITreeFilter
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        public bool IsEnabled { get; set; }
        public bool IncludeBots { get; set; }

        private List<ulong> _ignoredChannels;
        
        public List<ulong> IgnoredChannels { get {
            if (_ignoredChannels == null) _ignoredChannels = new List<ulong>();
            return _ignoredChannels;
        } private set {
            _ignoredChannels = value;
        }}

        public bool AddIgnoreChannel(ulong channelId)
        {
            if (IgnoredChannels.Contains(channelId)) return false;

            IgnoredChannels = new List<ulong>(IgnoredChannels);
            IgnoredChannels.Add(channelId);
            return true;
        }

        public bool RemoveIgnoreChannel(ulong channelId)
        {
            if (!IgnoredChannels.Contains(channelId)) return false;

            IgnoredChannels = new List<ulong>(IgnoredChannels);
            IgnoredChannels.RemoveAll(x => x == channelId);
            return true;            
        }
    }
}