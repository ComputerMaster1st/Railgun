using System.Collections.Generic;
using System.Linq;
using TreeDiagram.Models.SubModels;

namespace TreeDiagram.Models.Server.Warning
{
    public class ServerWarning : ConfigBase
    {
        public int WarnLimit { get; set; }
        public virtual List<ServerWarningInfo> Warnings { get; private set; } = new List<ServerWarningInfo>();

        public ServerWarning(ulong id) : base(id) { }
        
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