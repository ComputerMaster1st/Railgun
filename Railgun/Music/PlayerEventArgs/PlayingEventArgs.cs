using AudioChord;

namespace Railgun.Music.PlayerEventArgs
{
    public class PlayingEventArgs : PlayerEventArgs
    {
        public ISong Song { get; }
        public bool IsRatelimited { get; }

        public PlayingEventArgs(ulong guildId, ISong song, bool isRatelimited = false) : base(guildId)
        {
            Song = song;
            IsRatelimited = isRatelimited;
        }
    }
}