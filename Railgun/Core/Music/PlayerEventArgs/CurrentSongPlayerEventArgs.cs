using AudioChord;

namespace Railgun.Core.Music.PlayerEventArgs
{
    public class CurrentSongPlayerEventArgs : PlayerEventArgs
    {
        public ISong Song { get; }

        public CurrentSongPlayerEventArgs(ulong guildId, ISong song) : base(guildId) => Song = song;
    }
}