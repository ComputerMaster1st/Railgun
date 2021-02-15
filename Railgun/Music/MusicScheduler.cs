using AudioChord;
using Discord;
using MongoDB.Bson;
using Railgun.Core;
using Railgun.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos;

namespace Railgun.Music.Scheduler
{
    public class MusicScheduler
    {
        private readonly MusicServiceConfiguration _musicConfig;
        private readonly MusicService _musicService;
        private readonly ObjectId _playlistId;
        private readonly YoutubeClient _ytClient;
        private readonly MetaDataEnricher _enricher;
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

        public bool DisableShuffle { get; set; }  = false;

        public MusicScheduler(MusicServiceConfiguration musicConfig, MusicService musicService, ObjectId playlistId, bool playlistAutoLoop, YoutubeClient ytClient, MetaDataEnricher enricher, bool disableShuffle)
        {
            _musicConfig = musicConfig;
            _musicService = musicService;
            _playlistId = playlistId;
            PlaylistAutoLoop = playlistAutoLoop;
            _ytClient = ytClient;
            _enricher = enricher;
            DisableShuffle = disableShuffle;
        }

        public async Task<ISong> RequestNextSongAsync()
        {
            await _requestLock.WaitAsync();

            try
            {
                var playlist = await SystemUtilities.GetPlaylistAsync(_musicService, _playlistId);

                if (playlist == null) throw new MusicSchedulerException("No playlist has been found in the database. Please generate a new playlist.");

                PopulateSongList(playlist);

                var song = await FetchFromQueueAsync(playlist);

                if (!song.IsSuccess)
                    song = await FetchFromPlaylistAsync(playlist);

                if (!song.IsSuccess && song.Error == null && song.Song == null)
                    throw new NullReferenceException("No song to play! If you believe there is an issue, please report to our developers.");
                if (!song.IsSuccess && song.Error != null)
                    throw song.Error;
                
                return song.Song;
            }
            finally
            {
                _requestLock.Release();
            }
        }

        private async Task<(bool IsSuccess, Exception Error, ISong Song)> FetchFromPlaylistAsync(Playlist playlist)
        {
            (bool IsSuccess, Exception Error, ISong song) request = (false, null, null);

            if (_playlist.Count < 1)
                return request;

            // Check if all songs are either played or ratelimited. Convert played to queued.
            if (!_playlist.Any(x => x.Value == SongQueueStatus.Queued))
            {
                if (!PlaylistAutoLoop) return request;

                // All songs ratelimited, return null + error
                if (_playlist.Count > 0 && _playlist.All(x => x.Value == SongQueueStatus.RateLimited))
                {
                    request.Error = new MusicSchedulerException("Server playlist has been rate-limited (429) by YouTube.");
                    return request;
                }

                var playedSongs = _playlist.Where(x => x.Value == SongQueueStatus.Played).Select(x => x.Key).ToList();
                foreach (var loopSongId in playedSongs)
                    _playlist[loopSongId] = SongQueueStatus.Queued;
            }

            var songIds = _playlist.Where(x => x.Value == SongQueueStatus.Queued).Select(x => x.Key).ToList();
            var songId = DisableShuffle ? songIds.First() : songIds[_random.Next(0, songIds.Count())];
            request = await FetchSongAsync(songId);

            if (request.IsSuccess)
            {
                AssignQueueStatus(request.song.Metadata.Id, SongQueueStatus.Played);
                return request;
            }

            if (request.Error is RequestLimitExceededException)
            {
                AssignQueueStatus(songId, SongQueueStatus.RateLimited);
                return request;
            } 

            playlist.Songs.Remove(songId);
            _playlist.Remove(songId);
                    
            var ex = new SongQueueException($"A song failed to queue! - {songId}", request.Error);
            request.Error = ex;

            await SystemUtilities.UpdatePlaylistAsync(_musicService, playlist);
            return request;
        }

        private async Task<(bool IsSuccess, Exception Error, ISong Song)> FetchFromQueueAsync(Playlist playlist)
        {   
            (bool IsSuccess, Exception Error, ISong Song) song = (false, null, null);
            var request = Requests.FirstOrDefault();

            if (request == null) return song;

            if (request.Song != null)
            {
                Requests.RemoveAll(x => x.Id.ToString() == request.Id.ToString());
                AssignQueueStatus(request.Id, SongQueueStatus.Played);
                return (true, null, request.Song);
            }

            song = await FetchSongAsync(request.Id);

            if (song.IsSuccess) 
            {
                Requests.RemoveAll(x => x.Id.ToString() == request.Id.ToString());
                AssignQueueStatus(request.Id, SongQueueStatus.Played);
                return song;
            }

            if (song.Error is RequestLimitExceededException)
            {
                Requests.RemoveAll(x => x.Id.ToString() == request.Id.ToString());
                AssignQueueStatus(request.Id, SongQueueStatus.RateLimited);
                return song;
            }

            playlist.Songs.Remove(request.Id);
            _playlist.Remove(request.Id);
            Requests.RemoveAll(x => x.Id.ToString() == request.Id.ToString());
                    
            var ex = new SongQueueException($"A song failed to queue! - {request.Id}", song.Error);
            song.Error = ex;

            await SystemUtilities.UpdatePlaylistAsync(_musicService, playlist);
            return song;
        }

        private void AssignQueueStatus(SongId id, SongQueueStatus status)
        {
            if (_playlist.ContainsKey(id)) _playlist[id] = status;
        }

        private async Task<(bool IsSuccess, Exception Error, ISong Song)> FetchSongAsync(SongId id)
        {
            ISong result = await _musicService.GetSongAsync(id);
            if (result != null) return (true, null, result);

            Exception error = null;

            try
            {
                ISong song = null;

                if (id.ProcessorId == "DISCORD") return (false, null, null);

                var request = Requests.FirstOrDefault(f => f.Id == id);
				var ytUrl = "https://youtu.be/" + id.SourceId;
				string title;
				string uploader;

				if (request == null)
                {
					var videoId = VideoId.TryParse(ytUrl);
                    var video = await _ytClient.Videos.GetAsync(videoId.Value);

                    if (video.Duration > _musicConfig.ExtractorConfiguration.MaxSongDuration)
                        return (false, new SongQueueException(string.Format("Song ({0}) Exceeds Maximum Limit: {1} Minutes", Format.EscapeUrl(video.Url), _musicConfig.ExtractorConfiguration.MaxSongDuration.TotalMinutes)), null);

					title = video.Title;
					uploader = video.Author;
				}
                else
                {
					title = request.Name;
					uploader = request.Uploader;
                }

				_enricher.AddMapping(uploader, id, title);
				song = await _musicService.DownloadSongAsync(ytUrl);
                
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

            Requests.RemoveAll(x => x.Id.ToString() == song.Metadata.Id.ToString());

            _requestLock.Release();
        }
    }
}
