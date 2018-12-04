using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Discord.Audio;
using MongoDB.Bson;
using Railgun.Core.Enums;
using Railgun.Core.Music.PlayerEventArgs;

namespace Railgun.Core.Music
{
    public class Player
    {
        private IAudioClient _client = null;
        private Exception _audioDisconnectException = null;
        private bool _audioDisconnected = false;
        private bool _autoDisconnected = false;
        private bool _musicCancelled = false;
        private bool _streamCancelled = false;

        private readonly List<SongId> _playedSongs = new List<SongId>();

        private readonly MusicService _musicService;
        private ObjectId _playlistId = ObjectId.Empty;

        public IVoiceChannel VoiceChannel { get; }

        public Task PlayerTask { get; private set; } = null;
        public DateTime CreatedAt { get; } = DateTime.Now;
        public DateTime SongStartedAt { get; private set; }
        public List<ISong> Requests { get; } = new List<ISong>();
        public List<ulong> VoteSkipped { get; } = new List<ulong>();
        public PlayerStatus Status { get; private set; } = PlayerStatus.Idle;
        public bool AutoSkipped { get; set; } = false;
        public int RepeatSong { get; set; } = 0;
        public bool LeaveAfterSong { get; set; } = false;

        public event EventHandler<ConnectedPlayerEventArgs> Connected;
        public event EventHandler<CurrentSongPlayerEventArgs> Playing;
        public event EventHandler<TimeoutPlayerEventArgs> Timeout;
        public event EventHandler<FinishedPlayerEventArgs> Finished;

        public int Latency {
            get {
                if (_client != null) return _client.Latency;
                else return -1;
            }
        }

        public Player(MusicService musicService, IVoiceChannel vc) {
            _musicService = musicService;

            VoiceChannel = vc;
        }

        public void CancelMusic() => _musicCancelled = true;

        public void CancelStream(bool audioDisconnected = false) {
            _musicCancelled = true;
            _streamCancelled = true;
            _audioDisconnected = true;
        }

        public bool AddSongRequest(ISong song) {
            if (Requests.Contains(song)) return false;

            Requests.Add(song);
            return true;
        }

        public ISong GetFirstSongRequest() {
            if (Requests.Count < 1) return null;
            return Requests[0];
        }

        public void RemoveSongRequest(ISong song) {
            if (Requests.Contains(song)) Requests.Remove(song);
        }

        public bool VoteSkip(ulong userId) {
            if (VoteSkipped.Contains(userId)) return false;

            VoteSkipped.Add(userId);
            return true;
        }

        public async Task<int> GetUserCountAsync()
            => (await VoiceChannel.GetUsersAsync().FlattenAsync()).Count(user => !user.IsBot);

        public void StartPlayer(ObjectId playlistId, bool autoJoin) {
            _playlistId = playlistId;
            PlayerTask = Task.Run(() => StartAsync());
        }

        private async Task<bool> IsAloneAsync() {
            if (await GetUserCountAsync() < 1) return true;
            else return false;
        }

        private async Task<ISong> QueueSongAsync() {
            var request = GetFirstSongRequest();

            if (request != null) {
                if (!_playedSongs.Contains(request.Id)) _playedSongs.Add(request.Id);
                return request;
            }

            var playlist = await _musicService.Playlist.GetPlaylistAsync(_playlistId);

            if (playlist == null) return null;

            var rand = new Random();
            var remainingSongs = new List<SongId>(playlist.Songs);

            foreach (SongId songId in _playedSongs) {
                if (remainingSongs.Contains(songId)) remainingSongs.Remove(songId);
            }

            while (request == null) {
                if (remainingSongs.Count < 1) {
                    if (playlist.Songs.Count < 1) return null;

                    _playedSongs.Clear();
                    remainingSongs = new List<SongId>(playlist.Songs);
                }

                try {
                    var songId = remainingSongs[rand.Next(0, remainingSongs.Count)];

                    if (!await _musicService.TryGetSongAsync(songId, song => request = song)) {
                        playlist.Songs.Remove(songId);
                        remainingSongs.Remove(songId);

                        await _musicService.Playlist.UpdateAsync(playlist);
                    } else _playedSongs.Add(songId);
                } catch { }
            }

            return request;
        }

        private async Task TimeoutAsync(Task task, int ms, string errorMsg) {
            await Task.WhenAny(task, Task.Delay(ms));

            if (!task.IsCompleted) 
                throw new TimeoutException($"{errorMsg} (Task Status : {task.Status.ToString()})", task.Exception);
        }

        private async Task ConnectToVoiceAsync() {
            _client = await VoiceChannel.ConnectAsync();

            if (_client == null)
                throw new TimeoutException("Unable to establish a connection to voice server! Try changing regions if this problem persists.");
        }

        private async Task StartAsync() {
            Exception ex = null;
            Status = PlayerStatus.Connecting;

            try {
                await ConnectToVoiceAsync();

                Connected?.Invoke(this, new ConnectedPlayerEventArgs(VoiceChannel.GuildId));

                _client.Disconnected += (audioEx) => {
                    if (!_autoDisconnected && !_streamCancelled) {
                        _audioDisconnectException = audioEx;

                        CancelStream(true);
                    }

                    return Task.CompletedTask;
                };

                using (_client) 
                using (var discordStream = _client.CreateOpusStream()) {
                    Status = PlayerStatus.Connected;

                    while (!_streamCancelled) {
                        if (await IsAloneAsync() || LeaveAfterSong) {
                            _autoDisconnected = true;
                            break;
                        } else if (_musicCancelled) _musicCancelled = true;

                        Status = PlayerStatus.Queuing;

                        var song = await QueueSongAsync();

                        if (song == null) {
                            _autoDisconnected = true;
                            break;
                        }

                        AddSongRequest(song);

                        Playing?.Invoke(this, new CurrentSongPlayerEventArgs(VoiceChannel.GuildId, song.Id.ToString(), song.Metadata));

                        using (var databaseStream = await song.GetMusicStreamAsync())
                        using (var opusStream =  new OpusOggReadStream(databaseStream)) {
                            Status = PlayerStatus.Playing;
                            SongStartedAt = DateTime.Now;
                            Byte[] bytes;

                            while (opusStream.HasNextPacket && !_musicCancelled) {
                                bytes = opusStream.RetrieveNextPacket();

                                await TimeoutAsync(discordStream.WriteAsync(bytes, 0, bytes.Length), 5000, "WriteAsync has timed out!");
                            }

                            Status = PlayerStatus.Finishing;

                            await TimeoutAsync(discordStream.FlushAsync(), 5000, "FlushAsync has timed out!");
                        }

                        if (RepeatSong > 0) RepeatSong--;
                        else RemoveSongRequest(song);

                        if (AutoSkipped && Requests.Count < 1) AutoSkipped = false;
                        
                        VoteSkipped.Clear();
                    }

                    Status = PlayerStatus.Disconnecting;
                }
            } catch (TimeoutException timeEx) {
                Status = PlayerStatus.Timeout;

                Timeout?.Invoke(this, new TimeoutPlayerEventArgs(VoiceChannel.GuildId, timeEx));
            } catch (Exception inEx) {
                Status = PlayerStatus.FailSafe;
                ex = inEx;
            } finally {
                if (_audioDisconnected) {
                    ex = new Exception("AudioClient Unexpected Disconnect!", _audioDisconnectException);
                    _autoDisconnected = false;
                }

                Finished?.Invoke(this, new FinishedPlayerEventArgs(VoiceChannel.GuildId, _autoDisconnected, ex));

                Status = PlayerStatus.Disconnected;
            }
        }
    }
}