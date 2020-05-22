using System;

namespace Railgun.Music.PlayerEventArgs
{
    public class QueueFailEventArgs : PlayerEventArgs
    {
        public Exception Exception { get; }

        public QueueFailEventArgs(ulong guildId, Exception ex) : base(guildId) {
            Exception = ex;
        }
    }
}