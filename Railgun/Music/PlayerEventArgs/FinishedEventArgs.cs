using System;

namespace Railgun.Music.PlayerEventArgs
{
    public class FinishedEventArgs : PlayerEventArgs
    {
        public bool AutoDisconnected { get; }
        public Exception Exception { get; }

        public FinishedEventArgs(ulong guildId, bool autoDisconnected, Exception ex = null) : base(guildId) {
            AutoDisconnected = autoDisconnected;
            Exception = ex;
        }
    }
}