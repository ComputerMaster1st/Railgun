namespace TreeDiagram.Models.Server
{
    public class ServerCommand
    {
        public string Prefix { get; set; }
        public bool DeleteCmdAfterUse { get; set; }
        public bool RespondToBots { get; set; }
        public bool IgnoreModifiedMessages { get; set; }
    }
}