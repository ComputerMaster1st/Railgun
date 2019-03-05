﻿using System;

namespace TreeDiagram.Models
{
    public interface ITreeTimer
    {
        int Id { get; }

        ulong GuildId { get; }

        DateTime TimerExpire { get; }
    }
}
