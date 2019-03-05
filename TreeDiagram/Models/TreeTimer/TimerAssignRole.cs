namespace TreeDiagram.Models.TreeTimer
{
    public class TimerAssignRole : TimerBase
    {
        public ulong UserId { get; set; }
        public ulong RoleId { get; set; }
    }
}