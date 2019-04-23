using System;

namespace Railgun.Core.Music.PlayerEventArgs
{
    public class FinishedPlayerEventArgs : PlayerEventArgs
    {
        public bool AutoDisconnected { get; }
        public Exception Exception { get; }

        public FinishedPlayerEventArgs(ulong guildId, bool autoDisconnected, Exception ex = null) : base(guildId) {
            AutoDisconnected = autoDisconnected;
            Exception = ex;
        }
    }
}