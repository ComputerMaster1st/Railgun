namespace TreeDiagram.Models.Server
{
    public class ServerMention : ConfigBase
    {
        public bool DisableMentions { get; set; } = true;

        public ServerMention(ulong id) : base(id) { }
    }
}