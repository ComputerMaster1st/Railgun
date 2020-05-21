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

        /// <summary>
        /// This is a priority queue. This will always take priority over the dictionary.
        /// </summary>
        private readonly List<SongRequest> _requests = new List<SongRequest>();
        private readonly SemaphoreSlim _requestLock = new SemaphoreSlim(1);
        private readonly Random _random = new Random();

        /// <summary>
        /// This is a guild playlist which contains all songs currently up-to-date. The flags indicate whether it's ratelimited, played or is waiting to be randomly played.
        /// </summary>
        private Dictionary<SongId, SongQueueStatus> _playlist = new Dictionary<SongId, SongQueueStatus>();

        public bool PlaylistAutoLoop { get; set; }

        public bool IsRateLimited => _playlist.Any(x => x.Value == SongQueueStatus.RateLimited);

        public bool IsRequestsPopulated => _requests.Any();

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

                if (!song.IsSuccess && song.Error == null)
                    song = await FetchFromPlaylistAsync(playlist);

                return song.song ?? throw new MusicSchedulerException("Song Is Null!");
            }
            finally
            {
                _requestLock.Release();
            }
        }

        private async Task<(bool IsSuccess, Exception Error, ISong song)> FetchFromPlaylistAsync(Playlist playlist)
        {
            (bool IsSuccess, Exception Error, ISong song) request = (false, null, null);

            while (!request.IsSuccess)
            {
                // Check if all songs are either played or ratelimited. Convert played to queued.
                if ((_playlist.Count(x => x.Value == SongQueueStatus.Played) + _playlist.Count(x => x.Value == SongQueueStatus.RateLimited)) < 1)
                {
                    if (!PlaylistAutoLoop || !_playlist.Any()) return (false, null, null);

                    // All songs ratelimited, return null + error
                    if (_playlist.All(x => x.Value == SongQueueStatus.RateLimited)) throw new MusicSchedulerException("Server playlist has been rate-limited (429) by YouTube.");

                    foreach (var loopSongId in _playlist.Where(x => x.Value == SongQueueStatus.Played).Select(x => x.Key))
                        _playlist[loopSongId] = SongQueueStatus.Queued;
                }

                var songIds = _playlist.Where(x => x.Value == SongQueueStatus.Queued).Select(x => x.Key).ToList();
                var songId = songIds[_random.Next(0, songIds.Count())];
                var song = await FetchSongAsync(songId);

                if (song.IsSuccess)
                {
                    request = song;
                    break;
                }
                else if (song.Error is RequestLimitExceededException)
                {
                    _playlist[songId] = SongQueueStatus.RateLimited;
                    continue;
                }

                playlist.Songs.Remove(songId);
                _playlist.Remove(songId);

                await _musicService.Playlist.UpdateAsync(playlist);
            }

            _playlist[request.song.Id] = SongQueueStatus.Played;
            return request;
        }

        private async Task<(bool IsSuccess, Exception Error, ISong song)> FetchFromQueueAsync(Playlist playlist)
        {
            while (_requests.Count > 0)
            {
                var request = _requests.FirstOrDefault();
                if (request != null)
                {
                    if (request.Song != null)
                    {
                        _requests.RemoveAll(x => x.Id.ToString() == request.Id.ToString());
                        return (true, null, request.Song);
                    }
                    var fetchedSong = await FetchSongAsync(request.Id);

                    if (fetchedSong.IsSuccess) {
                        _requests.RemoveAll(x => x.Id.ToString() == request.Id.ToString());
                        return fetchedSong;
                    }
                    if (fetchedSong.Error is RequestLimitExceededException)
                    {
                        _playlist[request.Id] = SongQueueStatus.RateLimited;
                        _requests.RemoveAll(x => x.Id.ToString() == request.Id.ToString());
                        continue;
                    }

                    playlist.Songs.Remove(request.Id);
                    _playlist.Remove(request.Id);
                    _requests.RemoveAll(x => x.Id.ToString() == request.Id.ToString());

                    await _musicService.Playlist.UpdateAsync(playlist);
                }
            }

            return (false, null, null);
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

                if (!_requests.Any(x => x.Id == id))
                    song = await _musicService.DownloadSongAsync("https://youtu.be/" + id.SourceId);
                
                return (true, error, song);
            }
            catch (RequestLimitExceededException ex)
            {
                error = ex;
                SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, "Player", "FetchSongAsync Failed to get song!", ex));
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
            var repopulatedSongs = playlist.Songs.ToDictionary(x => x, y =>
            {
                return _playlist.GetValueOrDefault(y, SongQueueStatus.Queued);
            });

            _playlist = repopulatedSongs;
        }

        public async Task AddSongRequestAsync(SongRequest song)
        {
            await _requestLock.WaitAsync();

            if (_requests.Count(x => x.Id.ToString() == song.Id.ToString()) < 1)
                _requests.Add(song);

            _requestLock.Release();
        }

        public async Task RemoveSongRequestAsync(SongRequest song)
        {
            await _requestLock.WaitAsync();

            _requests.RemoveAll(x => x.Id.ToString() == song.Id.ToString());

            _requestLock.Release();
        }

        public async Task RemoveSongRequestAsync(ISong song)
        {
            await _requestLock.WaitAsync();

            _requests.RemoveAll(x => x.Id.ToString() == song.Id.ToString());

            _requestLock.Release();
        }
    }
}
