using System;

namespace Railgun.Music.PlayerEventArgs
{
    public class FinishedEventArgs : PlayerEventArgs
    {
        public DisconnectReason Reason { get; }
        public Exception Exception { get; }

        public FinishedEventArgs(ulong guildId, DisconnectReason reason, Exception ex = null) : base(guildId) {
            Reason = reason;
            Exception = ex;
        }
    }
}