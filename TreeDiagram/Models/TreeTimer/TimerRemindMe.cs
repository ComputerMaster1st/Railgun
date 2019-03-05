using System;

namespace TreeDiagram.Models.TreeTimer
{
    public class TimerRemindMe : TimerBase
    {
        public ulong TextChannelId { get; set; }
        public ulong UserId { get; set; }
        public string Message { get; set; }

        public TimerRemindMe(ulong guildId, DateTime timerExpire) : base(guildId, timerExpire) { }
    }
}