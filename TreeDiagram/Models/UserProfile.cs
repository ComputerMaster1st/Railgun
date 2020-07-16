using TreeDiagram.Models.User;

namespace TreeDiagram.Models
{
    public class UserProfile : IdBase
    {
        public virtual UserGlobals Globals { get; private set; } = new UserGlobals();

        public UserProfile(ulong id) : base(id) {}
    }
}