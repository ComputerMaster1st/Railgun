using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using TreeDiagram.Models.SubModels;

namespace TreeDiagram.Models.Server
{
    public class ServerWarning
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int WarnLimit { get; set; }
        public virtual List<ServerWarningInfo> Warnings { get; private set; } = new List<ServerWarningInfo>();
        
        public void AddWarning(ulong userId, string reason)
        {
            var warningInfo = Warnings.FirstOrDefault(info => info.UserId == userId) ?? new ServerWarningInfo(userId);
            warningInfo.Reasons.Add(reason);
            
            if (!Warnings.Exists(info => info.UserId == userId)) Warnings.Add(warningInfo);
        }

        public List<string> GetWarnings(ulong userId)
        {
            var warningInfo = Warnings.FirstOrDefault(info => info.UserId == userId);
            return warningInfo?.Reasons;
        }

        public void ResetWarnings(ulong userId)
        {
            Warnings.RemoveAll(info => info.UserId == userId);
        }
    }
}