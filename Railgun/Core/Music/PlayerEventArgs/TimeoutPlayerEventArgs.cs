using System;

namespace Railgun.Core.Music.PlayerEventArgs
{
    public class TimeoutPlayerEventArgs : PlayerEventArgs
    {
        public TimeoutException Exception { get; }

        public TimeoutPlayerEventArgs(ulong guildId, TimeoutException ex) : base(guildId) => Exception = ex;
    }
}