using System;

namespace Railgun.Music
{
    public class PlaylistEmptyException : Exception
    {
        public PlaylistEmptyException(string message) : base(message) { }

        public PlaylistEmptyException(string message, Exception innerException) : base(message, innerException) { }
    }
}