using AudioChord;
using Discord;
using Railgun.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TreeDiagram.Models.Server;

namespace Railgun.Music
{
    public class PlaylistResult
    {
        private readonly Dictionary<string, int> _errors = new Dictionary<string, int>();

        private readonly MusicService _musicService;
        private readonly ITextChannel _tc;
        private readonly ServerMusic _data;

        public PlaylistResult(MusicService service, ITextChannel tc, ServerMusic data)
        {
            _musicService = service;
            _tc = tc;
            _data = data;
        }

        public (int Failed, string Message) GetErrors()
        {
            if (_errors.Count < 1) return (0, null);

            var failed = 0;
            var output = new StringBuilder();

            foreach (var error in _errors)
            {
                failed += error.Value;
                output.AppendFormat("{0} x {1}", Format.Bold(error.Value.ToString()), error.Key).AppendLine();
            }

            return (failed, output.ToString());
        }

        public async Task ProcessPlaylistAsync(MusicController musicController, string url, Playlist playlist)
        {
            async void Handler(SongProcessStatus status) => await YoutubePlaylistStatusUpdatedAsync(status);
            var reporter = new Progress<SongProcessStatus>(Handler);

            var resolvingPlaylist = await _musicService.Youtube.DownloadPlaylistAsync(new Uri(url), reporter, CancellationToken.None);
            var queued = resolvingPlaylist.Songs.Count - resolvingPlaylist.ExistingSongs;
            var output = new StringBuilder()
                .AppendFormat("Found In Repository : {0}", Format.Bold(resolvingPlaylist.ExistingSongs.ToString()));

            if (queued > 0) output.AppendFormat(" {0} Queued For Installation : {1}", SystemUtilities.GetSeparator, Format.Bold(queued.ToString())).AppendLine();

            output.AppendLine("Processing of YouTube Playlists may take some time... Just to let you know.");

            await _tc.SendMessageAsync(output.ToString());
            await Task.Factory.StartNew(async () => await musicController.ProcessYoutubePlaylistAsync(url, playlist, resolvingPlaylist, _tc, this));
        }

        private async Task YoutubePlaylistStatusUpdatedAsync(SongProcessStatus status)
        {
            switch (status.Status)
            {
                case SongStatus.Errored:
                    {
                        var output = (SongProcessError)status;

                        foreach (var error in output.Exceptions.InnerExceptions)
                        {
                            if (!_errors.ContainsKey(error.Message))
                                _errors.Add(error.Message, 1);
                            else
                                _errors[error.Message]++;
                        }
                    }
                    break;
                case SongStatus.Processed:
                    {
                        var output = (SongProcessResult)status;
                        var song = await output.Result;

                        try
                        {
                            var playlist = await SystemUtilities.GetPlaylistAsync(_musicService, _data);

                            playlist.Songs.Add(song.Id);

                            await _musicService.Playlist.UpdateAsync(playlist);
                            await _tc.SendMessageAsync($"{Format.Bold("Encoded & Installed :")} ({song.Id}) {song.Metadata.Name}");
                        }
                        catch (ArgumentException ex)
                        {
                            SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, "Music Manager", "Missing Playlist", ex));
                        }
                        catch (Exception ex)
                        {
                            SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, "Music Manager", "Missing TC", ex));
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
