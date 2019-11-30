using AudioChord;
using System;

namespace Railgun.Music
{
    public class SongRequest
    {
        public SongId Id { get; }
        public string Name { get; }
        public TimeSpan Length { get; }

        public ISong Song { get; } = null;

        public SongRequest(ISong song)
        {
            Id = song.Id;
            Name = song.Metadata.Name;
            Length = song.Metadata.Length;
            Song = song;
        }

        public SongRequest(SongId id, string name, TimeSpan length)
        {
            Id = id;
            Name = name;
            Length = length;
        }
    }
}