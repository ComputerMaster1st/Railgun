using AudioChord;

namespace Railgun.Music.PlayerEventArgs
{
    public class PlayingEventArgs : PlayerEventArgs
    {
        public ISong Song { get; }

        public PlayingEventArgs(ulong guildId, ISong song) : base(guildId) => Song = song;
    }
}