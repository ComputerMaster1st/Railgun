namespace TreeDiagram.Models.TreeTimer
{
    public class TimerRemindMe : TimerBase
    {
        public ulong UserId { get; set; }
        public string Message { get; set; }
    }
}