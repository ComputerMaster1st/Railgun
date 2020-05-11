using AudioChord;
using System;

namespace Railgun.Music
{
    public class SongRequest
    {
        public SongId Id { get; }
        public string Name { get; }
        public TimeSpan Length { get; }
        public string Uploader { get;  }

        public ISong Song { get; } = null;

        public SongRequest(ISong song)
        {
            Id = song.Id;
            Name = song.Metadata.Title;
            Length = song.Metadata.Duration;
            Uploader = song.Metadata.Uploader;
            Song = song;
        }

        public SongRequest(SongId id, string name, TimeSpan length, string uploader)
        {
            Id = id;
            Name = name;
            Length = length;
            Uploader = uploader;
        }
    }
}