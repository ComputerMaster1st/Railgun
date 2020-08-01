using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models.SubModels
{
    public class ServerWarningInfo
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WarningId { get; private set; } = 0;
        public ulong UserId { get; private set; }

        private List<string> _reasons;

        public List<string> Reasons { 
            get {
                if (_reasons == null) _reasons = new List<string>();
                return _reasons;
            } set {
                _reasons = value;
            }}

        public ServerWarningInfo(ulong userId) {
            UserId = userId;
        }
    }
}