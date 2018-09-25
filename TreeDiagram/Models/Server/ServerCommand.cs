namespace TreeDiagram.Models.Server
{
    public class ServerCommand : ConfigBase
    {
        public string Prefix { get; set; } = null;
        public bool DeleteCmdAfterUse { get; set; } = false;
        public bool RespondToBots { get; set; } = false;

        public ServerCommand(ulong id) : base(id) { }
    }
}