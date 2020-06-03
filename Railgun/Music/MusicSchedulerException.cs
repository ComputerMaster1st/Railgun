using System;

namespace Railgun.Music
{
    public class MusicSchedulerException : Exception
    { 
        public MusicSchedulerException(string msg) : base(msg) { }
        
        public MusicSchedulerException(string msg, Exception innerException) : base(msg, innerException) { }
    }
}
