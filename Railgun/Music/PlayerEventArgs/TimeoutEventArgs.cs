using System;

namespace Railgun.Music.PlayerEventArgs
{
    public class TimeoutEventArgs : PlayerEventArgs
    {
        public TimeoutException Exception { get; }

        public TimeoutEventArgs(ulong guildId, TimeoutException ex) : base(guildId) => Exception = ex;
    }
}