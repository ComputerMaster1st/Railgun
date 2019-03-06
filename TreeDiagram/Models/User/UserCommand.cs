namespace TreeDiagram.Models.User
{
    public class UserCommand : ConfigBase
    {
        public string Prefix { get; set; }
        
        public UserCommand(ulong id) : base(id) { }
    }
}