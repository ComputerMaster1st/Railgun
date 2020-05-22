using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Discord.Audio;
using MongoDB.Bson;
using Railgun.Core.Enums;
using Railgun.Music.PlayerEventArgs;
using Railgun.Music.Scheduler;
using YoutubeExplode;
using YoutubeExplode.Exceptions;

namespace Railgun.Music
{
    public class Player
	{
		private IAudioClient _client;
		private Exception _exception;
		private bool _audioDisconnected;
		private DisconnectReason _disconnectReason = DisconnectReason.Manual;
		private bool _skipSong;
		private CancellationTokenSource _streamCancelled = new CancellationTokenSource();
		private readonly MusicService _musicService;
        private bool _nowRateLimited = false;

        public IVoiceChannel VoiceChannel { get; }
		public Task PlayerTask { get; private set; }
		public DateTime SongStartedAt { get; private set; }
		public List<ulong> VoteSkipped { get; } = new List<ulong>();
		public PlayerStatus Status { get; private set; } = PlayerStatus.Connecting;
		public bool AutoSkipped { get; set; }
		public int RepeatSong { get; set; }
		public bool LeaveAfterSong { get; set; }
		public ISong CurrentSong { get; private set; }
		public MusicScheduler MusicScheduler { get; }

		public event EventHandler<ConnectedEventArgs> Connected;
		public event EventHandler<PlayingEventArgs> Playing;
		public event EventHandler<QueueFailEventArgs> QueueFailed;
		public event EventHandler<FinishedEventArgs> Finished;

		public int Latency {
			get {
				if (_client != null) return _client.Latency;
				return -1;
			}
		}

		public Player(MusicService musicService, IVoiceChannel vc, MusicScheduler musicScheduler)
		{
			_musicService = musicService;
			VoiceChannel = vc;
			MusicScheduler = musicScheduler;
		}

		public void SkipMusic() => _skipSong = true;

		public void CancelStream(bool audioDisconnected = false)
		{
			_audioDisconnected = audioDisconnected;
			_streamCancelled.Cancel();
		}

        public bool VoteSkip(ulong userId)
		{
			if (VoteSkipped.Contains(userId)) return false;

			VoteSkipped.Add(userId);
			return true;
		}

		public async Task<int> GetUserCountAsync()
			=> (await VoiceChannel.GetUsersAsync().FlattenAsync()).Count(user => !user.IsBot);

        public void StartPlayer() => PlayerTask = Task.Run(StartAsync);

        private async Task<bool> IsAloneAsync() => await GetUserCountAsync() < 1;

		public async Task ConnectToVoiceAsync()
		{
			_client = await VoiceChannel.ConnectAsync();

			if (_client == null)
				throw new TimeoutException("Unable to establish a connection to voice server! Try changing regions if this problem persists.");
		}

		private async Task StartAsync()
		{
			Exception ex = null;

			try
			{
				Connected?.Invoke(this, new ConnectedEventArgs(VoiceChannel.GuildId));

				_client.Disconnected += (audioEx) =>
				{
					_exception = audioEx;
					CancelStream(true);

					return Task.CompletedTask;
				};

				using (_client)
				using (var discordStream = _client.CreateOpusStream())
				{
					Status = PlayerStatus.Connected;
					var onRepeat = false;

					while (!_streamCancelled.IsCancellationRequested)
					{
						if (await IsAloneAsync() || LeaveAfterSong)
						{
							_disconnectReason = DisconnectReason.Auto;
							break;
						}
						if (_skipSong) _skipSong = false;

						Status = PlayerStatus.Queuing;

						if (onRepeat)
							CurrentSong = await _musicService.GetSongAsync(CurrentSong.Id);
						else {
							try
                            {
								CurrentSong = await MusicScheduler.RequestNextSongAsync();
							}
							catch (RequestLimitExceededException) { continue; }
							catch (SongQueueException inEx)
							{
								QueueFailed?.Invoke(this, new QueueFailEventArgs(VoiceChannel.GuildId, inEx));
								continue;
							}
							catch (NullReferenceException inEx)
                            {
								ex = inEx;
								_disconnectReason = DisconnectReason.Auto;
								break;
							}
						}

						Status = PlayerStatus.Playing;
						SongStartedAt = DateTime.Now;
						Playing?.Invoke(this, new PlayingEventArgs(VoiceChannel.GuildId, CurrentSong, MusicScheduler.IsRateLimited && !_nowRateLimited));
						if (!_nowRateLimited && MusicScheduler.IsRateLimited) _nowRateLimited = true;

						using (var databaseStream = await CurrentSong.GetMusicStreamAsync())
						using (var opusStream = new OpusOggReadStream(databaseStream))
						{
							while (opusStream.HasNextPacket && !_skipSong && !_streamCancelled.IsCancellationRequested)
							{
								var bytes = opusStream.RetrieveNextPacket();
								await discordStream.WriteAsync(bytes, 0, bytes.Length, _streamCancelled.Token);
							}

							Status = PlayerStatus.Finishing;

							await discordStream.FlushAsync(_streamCancelled.Token);
						}

						if (RepeatSong > 0)
						{
							RepeatSong--;
							onRepeat = true;
						}
						else
							onRepeat = false;

						if (AutoSkipped && !MusicScheduler.IsRequestsPopulated) AutoSkipped = false;

						VoteSkipped.Clear();
					}

					Status = PlayerStatus.Disconnecting;
				}
			} catch (Exception inEx) {
				_disconnectReason = DisconnectReason.Exception;
				Status = PlayerStatus.FailSafe;
				ex = inEx;
			} finally {
				if (_audioDisconnected && !_streamCancelled.IsCancellationRequested) {
					ex = new Exception("AudioClient Unexpected Disconnect!", _exception);
					_disconnectReason = DisconnectReason.Exception;
				}
                
				Finished?.Invoke(this, new FinishedEventArgs(VoiceChannel.GuildId, _disconnectReason, ex));
				Status = PlayerStatus.Disconnected;
			}
		}
	}
}