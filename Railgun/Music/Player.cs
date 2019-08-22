using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Discord.Audio;
using MongoDB.Bson;
using Railgun.Core.Enums;
using Railgun.Music.PlayerEventArgs;

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
		private readonly List<SongId> _playedSongs = new List<SongId>();
        private List<SongId> _remainingSongs = new List<SongId>();

        public IVoiceChannel VoiceChannel { get; }
		public Task PlayerTask { get; private set; }
		public DateTime CreatedAt { get; } = DateTime.Now;
		public DateTime SongStartedAt { get; private set; }
		public List<ISong> Requests { get; } = new List<ISong>();
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

		public Player(MusicService musicService, IVoiceChannel vc)
		{
			_musicService = musicService;
			VoiceChannel = vc;
		}

		public void CancelMusic() => _musicCancelled = true;

		public void CancelStream(bool audioDisconnected = false)
		{
			_musicCancelled = true;
			_streamCancelled = true;
			_audioDisconnected = audioDisconnected;
		}

		public bool AddSongRequest(ISong song)
		{
			if (Requests.Any(x => x.Id == song.Id)) return false;

			Requests.Add(song);
			return true;
		}

		public ISong GetFirstSongRequest()
		{
			if (Requests.Count < 1) return null;
			return Requests[0];
		}

		public void RemoveSongRequest(ISong song)
		{
			if (Requests.Contains(song)) Requests.Remove(song);
		}

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

		private async Task<ISong> QueueSongAsync()
		{
			var request = GetFirstSongRequest();
			if (request != null) return request;

            var rand = new Random();
            Playlist playlist = null;
            var retry = 5;

            while (request == null) {
				if (_remainingSongs.Count == 0) {
                    playlist = await _musicService.Playlist.GetPlaylistAsync(_playlistId);

                    if (playlist == null || playlist.Songs.Count == 0) return null;
					if (!PlaylistAutoLoop) return null;

					_playedSongs.Clear();
					_remainingSongs = new List<SongId>(playlist.Songs);

                    foreach (SongId songId in _playedSongs) _remainingSongs.Remove(songId);
                }

				try
                {
                    var songId = _remainingSongs.Count == 1 ? _remainingSongs.First() : _remainingSongs[rand.Next(0, _remainingSongs.Count)];
                    var song = await _musicService.TryGetSongAsync(songId);

                    if (song.Item1)
                    {
                        request = song.Item2;
                        break;
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
                        return null;
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

					while (!_streamCancelled) {
						if (IsAlone() || LeaveAfterSong) {
							_autoDisconnected = true;
							break;
						}
						if (_musicCancelled) _musicCancelled = false;

						Status = PlayerStatus.Queuing;

						CurrentSong = await QueueSongAsync();

						if (CurrentSong == null) {
							_autoDisconnected = true;
							break;
						}

						AddSongRequest(CurrentSong);
						Status = PlayerStatus.Playing;
						SongStartedAt = DateTime.Now;
						Playing?.Invoke(this, new PlayingEventArgs(VoiceChannel.GuildId, CurrentSong));

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
                        if (RepeatSong > 0) RepeatSong--;
						else RemoveSongRequest(CurrentSong);

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