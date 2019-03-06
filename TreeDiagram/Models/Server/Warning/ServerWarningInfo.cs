using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models.Server.Warning
{
    public class ServerWarningInfo
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WarningId { get; private set; } = 0;
        public ulong UserId { get; private set; }
        public List<string> Reasons { get; set; } = new List<string>();

        public ServerWarningInfo(ulong userId) {
            UserId = userId;
        }
    }
}