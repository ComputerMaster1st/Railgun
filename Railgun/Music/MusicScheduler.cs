using AudioChord;
using Discord;
using MongoDB.Bson;
using Railgun.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Exceptions;

namespace Railgun.Music.Scheduler
{
    public class MusicScheduler
    {
        private readonly MusicService _musicService;
        private readonly ObjectId _playlistId;
        private readonly SemaphoreSlim _requestLock = new SemaphoreSlim(1);
        private readonly Random _random = new Random();

        /// <summary>
        /// This is a guild playlist which contains all songs currently up-to-date. The flags indicate whether it's ratelimited, played or is waiting to be randomly played.
        /// </summary>
        private Dictionary<SongId, SongQueueStatus> _playlist = new Dictionary<SongId, SongQueueStatus>();

        /// <summary>
        /// This is a priority queue. This will always take priority over the dictionary.
        /// </summary>
        public List<SongRequest> Requests { get; } = new List<SongRequest>();

        public bool PlaylistAutoLoop { get; set; }

        public bool IsRateLimited => _playlist.Any(x => x.Value == SongQueueStatus.RateLimited);

        public bool IsRequestsPopulated => Requests.Any();

        public MusicScheduler(MusicService musicService, ObjectId playlistId, bool playlistAutoLoop)
        {
            _musicService = musicService;
            _playlistId = playlistId;
            PlaylistAutoLoop = playlistAutoLoop;
        }

        public async Task<ISong> RequestNextSongAsync()
        {
            await _requestLock.WaitAsync();

            try
            {
                var playlist = await _musicService.Playlist.GetPlaylistAsync(_playlistId);

                if (playlist == null) throw new MusicSchedulerException("No playlist has been found in the database. Please generate a new playlist.");

                PopulateSongList(playlist);

                var song = await FetchFromQueueAsync(playlist);

                if (!song.IsSuccess)
                    song = await FetchFromPlaylistAsync(playlist);

                if (!song.IsSuccess && song.Error == null)
                    throw new NullReferenceException("No song to play!");
                if (!song.IsSuccess && song.Error != null)
                    throw song.Error;
                
                return song.song;
            }
            finally
            {
                _requestLock.Release();
            }
        }

        private async Task<(bool IsSuccess, Exception Error, ISong song)> FetchFromPlaylistAsync(Playlist playlist)
        {
            (bool IsSuccess, Exception Error, ISong song) request = (false, null, null);

            // Check if all songs are either played or ratelimited. Convert played to queued.
            if (!_playlist.Any(x => x.Value == SongQueueStatus.Queued))
            {
                if (!PlaylistAutoLoop) return request;

                // All songs ratelimited, return null + error
                if (_playlist.All(x => x.Value == SongQueueStatus.RateLimited)) 
                    throw new MusicSchedulerException("Server playlist has been rate-limited (429) by YouTube.");

                foreach (var loopSongId in _playlist.Where(x => x.Value == SongQueueStatus.Played).Select(x => x.Key))
                    _playlist[loopSongId] = SongQueueStatus.Queued;
            }

            var songIds = _playlist.Where(x => x.Value == SongQueueStatus.Queued).Select(x => x.Key).ToList();
            var songId = songIds[_random.Next(0, songIds.Count())];
            request = await FetchSongAsync(songId);

            if (request.IsSuccess)
            {
                _playlist[request.song.Id] = SongQueueStatus.Played;
                return request;
            }

            if (request.Error is RequestLimitExceededException)
            {
                _playlist[songId] = SongQueueStatus.RateLimited;
                return request;
            } 

            playlist.Songs.Remove(songId);
            _playlist.Remove(songId);
                    
            var ex = new SongQueueException($"A song failed to queue! - {songId}", request.Error);
            request.Error = ex;

            await _musicService.Playlist.UpdateAsync(playlist);
            return request;
        }

        private async Task<(bool IsSuccess, Exception Error, ISong song)> FetchFromQueueAsync(Playlist playlist)
        {   
            (bool IsSuccess, Exception Error, ISong song) song = (false, null, null);
            var request = Requests.FirstOrDefault();

            if (request == null) return song;

            if (request.Song != null)
            {
                Requests.RemoveAll(x => x.Id.ToString() == request.Id.ToString());
                _playlist[request.Id] = SongQueueStatus.Played;
                return (true, null, request.Song);
            }

            song = await FetchSongAsync(request.Id);

            if (song.IsSuccess) 
            {
                Requests.RemoveAll(x => x.Id.ToString() == request.Id.ToString());
                _playlist[request.Id] = SongQueueStatus.Played;
                return song;
            }

            if (song.Error is RequestLimitExceededException)
            {
                Requests.RemoveAll(x => x.Id.ToString() == request.Id.ToString());
                _playlist[request.Id] = SongQueueStatus.RateLimited;
                return song;
            }

            playlist.Songs.Remove(request.Id);
            _playlist.Remove(request.Id);
            Requests.RemoveAll(x => x.Id.ToString() == request.Id.ToString());
                    
            var ex = new SongQueueException($"A song failed to queue! - {request.Id}", song.Error);
            song.Error = ex;

            await _musicService.Playlist.UpdateAsync(playlist);
            return song;
        }

        private async Task<(bool IsSuccess, Exception Error, ISong Song)> FetchSongAsync(SongId id)
        {
            (bool Success, ISong Song) result = await _musicService.TryGetSongAsync(id);
            if (result.Success) return (result.Success, null, result.Song);

            Exception error = null;

            try
            {
                ISong song = null;

                if (id.ProcessorId == "DISCORD") return (false, null, null);

                if (!Requests.Any(x => x.Id == id))
                    song = await _musicService.DownloadSongAsync("https://youtu.be/" + id.SourceId);
                
                return (true, error, song);
            }
            catch (RequestLimitExceededException ex)
            {
                error = ex;
                SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, "Player", "Youtube Rate-Limited (429)!", ex));
            }
            catch (Exception ex)
            {
                error = ex;
                SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, "Player", "FetchSongAsync Failed to get song!", ex));
            }

            return (false, error, null);
        }

        private void PopulateSongList(Playlist playlist)
        {
            var repopulatedSongs = playlist.Songs.Distinct().ToDictionary(x => x, y =>
            {
                return _playlist.GetValueOrDefault(y, SongQueueStatus.Queued);
            });

            _playlist = repopulatedSongs;
        }

        public async Task AddSongRequestAsync(SongRequest song)
        {
            await _requestLock.WaitAsync();

            if (Requests.Count(x => x.Id.ToString() == song.Id.ToString()) < 1)
                Requests.Add(song);

            _requestLock.Release();
        }

        public async Task RemoveSongRequestAsync(SongRequest song)
        {
            await _requestLock.WaitAsync();

            Requests.RemoveAll(x => x.Id.ToString() == song.Id.ToString());

            _requestLock.Release();
        }

        public async Task RemoveSongRequestAsync(ISong song)
        {
            await _requestLock.WaitAsync();

            Requests.RemoveAll(x => x.Id.ToString() == song.Id.ToString());

            _requestLock.Release();
        }
    }
}
