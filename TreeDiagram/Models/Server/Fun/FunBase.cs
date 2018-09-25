namespace TreeDiagram.Models.Server.Fun
{
    public abstract class FunBase : ConfigBase
    {
        public bool IsEnabled { get; set; } = true;

        protected FunBase(ulong id) : base(id) { }
    }
}