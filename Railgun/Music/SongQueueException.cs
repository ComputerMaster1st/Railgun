using System;

namespace Railgun.Music
{
    public class SongQueueException : Exception
    {
        public SongQueueException(string message) : base(message) { }

        public SongQueueException(string message, Exception innerException) : base(message, innerException) { }
    }
}