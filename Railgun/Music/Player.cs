using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Discord.Audio;
using MongoDB.Bson;
using Railgun.Core;
using Railgun.Core.Enums;
using Railgun.Music.PlayerEventArgs;
using YoutubeExplode;
using YoutubeExplode.Exceptions;

namespace Railgun.Music
{
	public class Player
	{
		private IAudioClient _client;
		private Exception _exception;
		private bool _audioDisconnected;
		private bool _autoDisconnected;
		private bool _musicCancelled;
		private bool _streamCancelled;
        private bool _queueFailed;
		private ObjectId _playlistId = ObjectId.Empty;
		private readonly MusicService _musicService;
		private readonly YoutubeMetaDataEnricher _enricher;
		private readonly List<SongId> _playedSongs = new List<SongId>();
        private List<SongId> _remainingSongs = new List<SongId>();
		private List<SongId> _rateLimited = new List<SongId>();
        private bool _nowRateLimited = false;
		private bool _firstLoop = true;

        public IVoiceChannel VoiceChannel { get; }
		public Task PlayerTask { get; private set; }
		public DateTime CreatedAt { get; } = DateTime.Now;
		public DateTime SongStartedAt { get; private set; }
		public List<SongRequest> Requests { get; } = new List<SongRequest>();
		public List<ulong> VoteSkipped { get; } = new List<ulong>();
		public PlayerStatus Status { get; private set; } = PlayerStatus.Connecting;
		public bool AutoSkipped { get; set; }
		public bool PlaylistAutoLoop { get; set; } = true;
		public int RepeatSong { get; set; }
		public bool LeaveAfterSong { get; set; }
		public ISong CurrentSong { get; private set; }

		public event EventHandler<ConnectedEventArgs> Connected;
		public event EventHandler<PlayingEventArgs> Playing;
		public event EventHandler<TimeoutEventArgs> Timeout;
		public event EventHandler<FinishedEventArgs> Finished;

		public int Latency {
			get {
				if (_client != null) return _client.Latency;
				return -1;
			}
		}

		public Player(MusicService musicService, IVoiceChannel vc, YoutubeMetaDataEnricher enricher)
		{
			_musicService = musicService;
			VoiceChannel = vc;
			_enricher = enricher;
		}

		public void CancelMusic() => _musicCancelled = true;

		public void CancelStream(bool audioDisconnected = false)
		{
			_musicCancelled = true;
			_streamCancelled = true;
			_audioDisconnected = audioDisconnected;
		}

		public bool AddSongRequest(SongRequest song)
		{
			if (Requests.Count(x => x.Id == song.Id) > 0) return false;

			Requests.Add(song);
			return true;
		}

        public SongRequest GetFirstSongRequest() => Requests.FirstOrDefault();

        public void RemoveSongRequest(SongRequest song) => Requests.RemoveAll(x => x.Id.ToString() == song.Id.ToString());

        public void RemoveSongRequest(ISong song) => Requests.RemoveAll(x => x.Id.ToString() == song.Id.ToString());

        public bool VoteSkip(ulong userId)
		{
			if (VoteSkipped.Contains(userId)) return false;

			VoteSkipped.Add(userId);
			return true;
		}

		public int GetUserCount()
			=> (VoiceChannel.GetUsersAsync().FlattenAsync().GetAwaiter().GetResult()).Count(user => !user.IsBot);

		public void StartPlayer(ObjectId playlistId)
		{
			_playlistId = playlistId;
			PlayerTask = Task.Run(StartAsync);
		}

		private bool IsAlone()
		{
			if (GetUserCount() < 1) return true;
			return false;
		}

		private async Task<(bool IsSuccess, string Error, ISong Song)> FetchSongAsync(SongId id)
		{
			(bool Success, ISong Song) result = await _musicService.TryGetSongAsync(id);
			if (result.Success) return (result.Success, string.Empty, result.Song);

			var isSuccess = false;
			ISong song = null;
			var error = string.Empty;

			try 
			{
				if (id.ProcessorId == "DISCORD") return (isSuccess, error, song);

				var request = Requests.FirstOrDefault(f => f.Id == id);
				var ytUrl = "https://youtu.be/" + id.SourceId;
				string title;
				string uploader;

				if (request == null)
                {
					var videoId = YoutubeExplode.Videos.VideoId.TryParse(ytUrl);
					var video = await new YoutubeClient().Videos.GetAsync(videoId.Value);

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
				isSuccess = true;
			}
			catch (RequestLimitExceededException ex)
			{
				error = "Youtube Rate-Limited (Error Code: 429)";
				SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, "Player", "FetchSongAsync Failed to get song!", ex));
			}
			catch (Exception ex)
            {
				error = ex.Message;
				SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, "Player", "FetchSongAsync Failed to get song!", ex));
			}
			
			return (isSuccess, error, song);
		}

        private async Task<(bool IsSuccess, string Error, ISong song)> QueueFirstRequestedSong(Playlist playlist)
        {
            while (Requests.Count > 0)
            {
                var request = GetFirstSongRequest();
                if (request != null)
                {
                    if (request.Song != null) return (true, string.Empty, request.Song);
                    var fetchedSong = await FetchSongAsync(request.Id);

                    if (fetchedSong.IsSuccess) return fetchedSong;
                    if (fetchedSong.Error.Contains("Error Code: 429"))
                    {
                        _remainingSongs.Remove(request.Id);
                        _rateLimited.Add(request.Id);
                        RemoveSongRequest(request);
                        continue;
                    }

                    playlist.Songs.Remove(request.Id);
                    _remainingSongs.Remove(request.Id);
                    RemoveSongRequest(request);

                    await _musicService.Playlist.UpdateAsync(playlist);
                }
            }

            return (false, null, null);
        }

		private async Task<(bool IsSuccess, string Error, ISong song)> QueueSongAsync()
        {
            Playlist playlist = await _musicService.Playlist.GetPlaylistAsync(_playlistId);
            var request = await QueueFirstRequestedSong(playlist);
            var rand = new Random();
            var retry = 5;

            while (!request.IsSuccess && string.IsNullOrEmpty(request.Error)) {
				if (_remainingSongs.Count < 1) {
                    playlist = await _musicService.Playlist.GetPlaylistAsync(_playlistId);

                    if (playlist == null || playlist.Songs.Count == 0) return (false, null, null);
					if (!PlaylistAutoLoop && !_firstLoop) return (false, null, null);
					if (playlist.Songs.Count <= _rateLimited.Count) return (false, "Playlist:429", null);
					
					_playedSongs.Clear();
                    _rateLimited.Clear();
					_remainingSongs = new List<SongId>(playlist.Songs);
					_firstLoop = false;

                    foreach (SongId songId in _playedSongs) _remainingSongs.Remove(songId);
                }

				try
                {
                    var songId = _remainingSongs.Count < 2 ? _remainingSongs.First() : _remainingSongs[rand.Next(0, _remainingSongs.Count)];
                    var song = await FetchSongAsync(songId);

                    if (song.IsSuccess)
                    {
                        request = song;
                        break;
                    }
					else if (song.Error.Contains("Response status code does not indicate success: 429"))
					{
                    	_remainingSongs.Remove(songId);
						_rateLimited.Add(songId);
						continue;
					}

                    playlist.Songs.Remove(songId);
                    _remainingSongs.Remove(songId);

                    await _musicService.Playlist.UpdateAsync(playlist);
                }
                catch (Exception e) {
                    retry--;
                    if (retry == 0) {
                        _exception = e;
                        _queueFailed = true;
                        return (false, null, null);
                    }
				}
			}

            return request;
		}

		private async Task ForceTimeout(Task task, int ms, string errorMsg)
		{
			await Task.WhenAny(task, Task.Delay(ms));

			if (!task.IsCompleted)
				throw new TimeoutException($"{errorMsg} (Task Status : {task.Status.ToString()})", task.Exception);
		}

		public async Task ConnectToVoiceAsync()
		{
			_client = await VoiceChannel.ConnectAsync();

			if (_client == null)
				throw new TimeoutException("Unable to establish a connection to voice server! Try changing regions if this problem persists.");
		}

		private async Task StartAsync()
		{
			Exception ex = null;

			try {
				Connected?.Invoke(this, new ConnectedEventArgs(VoiceChannel.GuildId));

				_client.Disconnected += (audioEx) => {
					if (!_autoDisconnected && !_streamCancelled) {
						_exception = audioEx;

						CancelStream(true);
					}

					return Task.CompletedTask;
				};

				using (_client)
				using (var discordStream = _client.CreateOpusStream()) {
					Status = PlayerStatus.Connected;
					var onRepeat = false;

					while (!_streamCancelled) {
						if (IsAlone() || LeaveAfterSong) {
							_autoDisconnected = true;
							break;
						}
						if (_musicCancelled) _musicCancelled = false;

						Status = PlayerStatus.Queuing;

						if (onRepeat) CurrentSong = await _musicService.GetSongAsync(CurrentSong.Id);
						else
						{
							var result = await QueueSongAsync();
							if (result.IsSuccess) CurrentSong = result.song;
							else
							{
								if (result.Error == "Playlist:429")
									throw new Exception("YouTube (429) block in effect. No music in the playlist can be played. Sorry. Please allow upto 24 hours for the block to clear.");
							}
						}
						if (CurrentSong == null) {
							_autoDisconnected = true;
							break;
						}

						Status = PlayerStatus.Playing;
						SongStartedAt = DateTime.Now;
						Playing?.Invoke(this, new PlayingEventArgs(VoiceChannel.GuildId, CurrentSong, (_rateLimited.Count > 0) ? (!_nowRateLimited ? true : false) : false));
                        if (!_nowRateLimited && _rateLimited.Count > 0) _nowRateLimited = true;

						using (var databaseStream = await CurrentSong.GetMusicStreamAsync())
						using (var opusStream = new OpusOggReadStream(databaseStream)) {
							while (opusStream.HasNextPacket && !_musicCancelled) {
								var bytes = opusStream.RetrieveNextPacket();
								await ForceTimeout(discordStream.WriteAsync(bytes, 0, bytes.Length), 5000, "WriteAsync has timed out!");
							}

							Status = PlayerStatus.Finishing;

							await ForceTimeout(discordStream.FlushAsync(), 5000, "FlushAsync has timed out!");
						}

            			if (!_playedSongs.Contains(CurrentSong.Id)) _playedSongs.Add(CurrentSong.Id);
            			if (_remainingSongs.Contains(CurrentSong.Id)) _remainingSongs.Remove(CurrentSong.Id);
                        if (RepeatSong > 0) {
							RepeatSong--;
							onRepeat = true;
						}
						else {
							RemoveSongRequest(CurrentSong);
							onRepeat = false;
						}

						if (AutoSkipped && Requests.Count < 1) AutoSkipped = false;

						VoteSkipped.Clear();
					}

					Status = PlayerStatus.Disconnecting;
				}
			} catch (TimeoutException timeEx) {
				Status = PlayerStatus.Timeout;
				Timeout?.Invoke(this, new TimeoutEventArgs(VoiceChannel.GuildId, timeEx));
			} catch (Exception inEx) {
				Status = PlayerStatus.FailSafe;
				ex = inEx;
			} finally {
				if (_audioDisconnected) {
					ex = new Exception("AudioClient Unexpected Disconnect!", _exception);
					_autoDisconnected = false;
				} else if (_queueFailed) {
                    ex = new Exception("Music Auto-Selector Failed!", _exception);
                    _autoDisconnected = false;
                }
                
				Finished?.Invoke(this, new FinishedEventArgs(VoiceChannel.GuildId, _autoDisconnected, ex));
				Status = PlayerStatus.Disconnected;
			}
		}
	}
}