using System;

namespace TreeDiagram.Models
{
    public interface ITreeTimer
    {
        int Id { get; }

        ulong GuildId { get; set; }
        ulong TextChannelId { get; set; }

        DateTime TimerExpire { get; set; }
    }
}
