namespace TreeDiagram.Models.User
{
    public class UserMention : ConfigBase
    {
        public bool DisableMentions { get; set; }
        
        public UserMention(ulong id) : base(id) { }
    }
}