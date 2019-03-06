using System;

namespace TreeDiagram.Interfaces
{
    public interface ITreeTimer
    {
        int Id { get; }

        ulong GuildId { get; }

        DateTime TimerExpire { get; }
    }
}
