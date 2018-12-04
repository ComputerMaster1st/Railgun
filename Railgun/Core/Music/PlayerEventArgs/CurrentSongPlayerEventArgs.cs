using AudioChord;

namespace Railgun.Core.Music.PlayerEventArgs
{
    public class CurrentSongPlayerEventArgs : PlayerEventArgs
    {
        public string SongId { get; }
        public SongMetadata Metadata { get; }

        public CurrentSongPlayerEventArgs(ulong guildId, string songId, SongMetadata metaData) : base(guildId) {
            SongId = songId;
            Metadata = metaData;
        }
    }
}