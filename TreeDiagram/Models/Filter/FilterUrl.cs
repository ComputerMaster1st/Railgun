using System.Collections.Generic;

namespace TreeDiagram.Models.Filter
{
    public class FilterUrl : FilterBase
    {
        public bool BlockServerInvites { get; set; }
        public bool DenyMode { get; set; }

        private List<string> _bannedUrls;

        public List<string> BannedUrls { 
            get {
                if (_bannedUrls == null) _bannedUrls = new List<string>();
                return _bannedUrls;
            } private set {
                _bannedUrls = value;
            }}
    }
}