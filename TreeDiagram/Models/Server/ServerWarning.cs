using System.Collections.Generic;

namespace TreeDiagram.Models.Server
{
    public class ServerWarning : ConfigBase
    {
        public int WarnLimit { get; set; } = 0;
        public Dictionary<ulong, List<string>> Warnings { get; private set; } = new Dictionary<ulong, List<string>>();

        public ServerWarning(ulong id) : base(id) { }
        
        public void AddWarning(ulong userId, string reason) {
            if (!Warnings.ContainsKey(userId))
                Warnings.Add(userId, new List<string>());

            Warnings[userId].Add(reason);
        }

        public List<string> GetWarnings(ulong userId)
            => Warnings.ContainsKey(userId) ? Warnings[userId] : null;

        public void ResetWarnings(ulong userId) {
            if (Warnings.ContainsKey(userId)) Warnings.Remove(userId);
        }
    }
}