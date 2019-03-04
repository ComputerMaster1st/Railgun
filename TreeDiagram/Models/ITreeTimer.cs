using System;

namespace TreeDiagram.Models
{
    public interface ITreeTimer
    {
        double Id { get; }

        ulong GuildId { get; set; }
        ulong TextChannelId { get; set; }

        DateTime TimerExpire { get; set; }
    }
}
