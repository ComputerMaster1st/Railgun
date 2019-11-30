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
    }
}