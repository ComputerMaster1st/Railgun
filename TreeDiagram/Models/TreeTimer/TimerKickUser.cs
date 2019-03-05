using System;

namespace TreeDiagram.Models.TreeTimer
{
    public class TimerKickUser : TimerBase
    {
        public ulong UserId { get; set; }

        public TimerKickUser(ulong guildId, DateTime timerExpire) : base(guildId, timerExpire) {}
    }
}