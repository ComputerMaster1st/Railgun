using AudioChord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Railgun.Music
{
    public class DiscordMetaDataEnricher : IAudioMetadataEnricher
    {
        private Dictionary<SongId, string> _mapping = new Dictionary<SongId, string>();

        public void AddMapping(string user, ulong id) => _mapping.Add(SongId.Parse($"DISCORD#{id}"), user);

        public Task<ISong> EnrichAsync(ISong song)
        {
            song.Metadata.Uploader = _mapping.GetValueOrDefault(song.Id);
            _mapping.Remove(song.Id);
            return Task.FromResult(song);
        }
    }
}
