﻿using AudioChord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Railgun.Music
{
    public class DiscordMetaDataEnricher : IAudioMetadataEnricher
    {
        private Dictionary<SongId, (string Username, string Title)> _mapping = new Dictionary<SongId, (string, string)>();

        public void AddMapping(string user, ulong id, string title) => _mapping.Add(SongId.Parse($"DISCORD#{id}"), (user, title));

        public Task<ISong> EnrichAsync(ISong song)
        {
            song.Metadata.Uploader = _mapping.GetValueOrDefault(song.Id).Username;
            song.Metadata.Title = _mapping.GetValueOrDefault(song.Id).Title;
            _mapping.Remove(song.Id);
            return Task.FromResult(song);
        }
    }
}