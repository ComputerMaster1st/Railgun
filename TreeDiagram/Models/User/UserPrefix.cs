namespace TreeDiagram.Models.User
{
    public class UserPrefix : ConfigBase
    {
        public string Prefix { get; set; } = null;
        
        public UserPrefix(ulong id) : base(id) { }
    }
}