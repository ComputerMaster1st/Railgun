using System;

namespace TreeDiagram.Models.TreeTimer
{
    public class TimerAssignRole : TimerBase
    {
        public ulong UserId { get; set; }
        public ulong RoleId { get; set; }

        public TimerAssignRole(ulong guildId, ulong tc, DateTime timerExpire) : base(guildId, tc, timerExpire) {}
    }
}