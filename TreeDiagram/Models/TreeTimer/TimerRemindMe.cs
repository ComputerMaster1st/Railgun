using System;

namespace TreeDiagram.Models.TreeTimer
{
    public class TimerRemindMe : TimerBase
    {
        public ulong UserId { get; set; }
        public string Message { get; set; }

        public TimerRemindMe(ulong guildId, ulong tc, DateTime timerExpire) : base(guildId, tc, timerExpire) {}
    }
}