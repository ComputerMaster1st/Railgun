using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Logging;
using Railgun.Core.Utilities;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Core.Managers
{
    public class MusicManager
    {
        private readonly IServiceProvider _services;
        private readonly Log _log;
        private readonly CommandUtils _commandUtils;
        private readonly MusicService _musicService;

        public MusicManager(IServiceProvider services) {
            _services = services;

            _log = _services.GetService<Log>();
            _commandUtils = _services.GetService<CommandUtils>();
            _musicService = _services.GetService<MusicService>();
        }

        public async Task AddYoutubeSongsAsync(List<string> urls, ITextChannel tc) {
            Playlist playlist;

            using (var scope = _services.CreateScope()) {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
                var data = await db.ServerMusics.GetOrCreateAsync(tc.GuildId);
                playlist = await _commandUtils.GetPlaylistAsync(data);

                await db.SaveChangesAsync();
            }

            var playlistModified = false;

            foreach (var url in urls) {
                var response = await tc.SendMessageAsync($"{Format.Bold("Processing :")} <{url}>...");
                var cleanUrl = url.Trim(' ', '<', '>');
                var videoId = string.Empty;
                ISong song = null;

                if (!_musicService.Youtube.TryParseYoutubeUrl(url, out videoId)) {
                    await response.ModifyAsync(properties => properties.Content = $"{Format.Bold("Invalid Url :")} <{cleanUrl}>");
                    continue;
                } else if (await _musicService.TryGetSongAsync(new SongId("YOUTUBE", videoId), result => song = result)) {
                    if (playlist.Songs.Contains(song.Id))
                        await response.ModifyAsync(properties => properties.Content = $"{Format.Bold("Already Installed :")} ({song.Id.ToString()}) {song.Metadata.Name}");
                    else {
                        playlist.Songs.Add(song.Id);
                        playlistModified = true;

                        await response.ModifyAsync(properties => properties.Content = $"{Format.Bold("Installed :")} ({song.Id.ToString()}) {song.Metadata.Name}");
                    }

                    continue;
                }

                try {
                    song = await _musicService.Youtube.DownloadAsync(new Uri(url));
                    playlist.Songs.Add(song.Id);
                    playlistModified = true;

                    await response.ModifyAsync(properties => properties.Content = $"{Format.Bold("Encoded & Installed :")} ({song.Id.ToString()}) {song.Metadata.Name}");
                } catch (Exception ex) {
                    await response.ModifyAsync(properties => properties.Content = $"{Format.Bold("Failed To Install :")} (<{cleanUrl}>), {ex.Message}");
                }
            }

            if (playlistModified) await _musicService.Playlist.UpdateAsync(playlist);

            await tc.SendMessageAsync("Done!");
        }

        public async Task ProcessYoutubePlaylistAsync(Playlist playlist, ResolvingPlaylist resolvingPlaylist, ITextChannel tc) {
            var alreadyInstalled = 0;
            var failed = 0;

            foreach (var songTask in resolvingPlaylist.Songs) {
                try {
                    var song = songTask.Result;

                    if (playlist.Songs.Contains(song.Id)) {
                        alreadyInstalled++;
                        continue;
                    }

                    playlist.Songs.Add(song.Id);

                    await _musicService.Playlist.UpdateAsync(playlist);
                } catch { failed++; }
            }

            var output = new StringBuilder()
                .AppendLine("Finished Processing YouTube Playlist! Results...")
                .AppendFormat(
                "Already Installed : {0} || Imported From Repository : {1} || Newly Encoded : {2} || Failed : {3}",
                    Format.Bold(alreadyInstalled.ToString()),
                    Format.Bold(resolvingPlaylist.ExistingSongs.ToString()),
                    Format.Bold(((resolvingPlaylist.Songs.Count - resolvingPlaylist.ExistingSongs) - failed).ToString()),
                    Format.Bold(failed.ToString())
                );
            
            await tc.SendMessageAsync(output.ToString());
        }

        public async Task YoutubePlaylistStatusUpdatedAsync(ITextChannel tc, SongProcessStatus status, ServerMusic data) {
            switch (status.Status) {
                case SongStatus.Errored:
                    {
                        var output = (SongProcessError)status;
                        var url = "https://youtu.be/" + output.Id.SourceId;
                        var logOutput = new StringBuilder()
                                .AppendFormat("<{0} <{1}>> Process Failure!", tc.Guild.Name, tc.GuildId).AppendLine()
                                .AppendFormat("{0} - {1}", url, output.Exceptions.Message);

                        try {
                            await tc.SendMessageAsync($"{Format.Bold("Failed To Install :")} (<{url}>), {output.Exceptions.Message}");
                        } catch (ArgumentException ex) {
                            await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "Music Manager", "Missing Playlist", ex));
                        } catch (Exception ex) {
                            await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "Music Manager", "Missing TC", ex));
                        }

                        await _log.LogToBotLogAsync(logOutput.ToString(), BotLogType.AudioChord);
                    }
                    break;
                case SongStatus.Processed:
                    {
                        var output = (SongProcessResult)status;
                        var song = await output.Result;
                        
                        var logOutput = new StringBuilder()
                            .AppendFormat("<{0} ({1})> Processed Song!", tc.Guild.Name, tc.GuildId).AppendLine()
                            .AppendFormat("{0} <{1}> - {2}", song.Id.ToString(), song.Metadata.Length.ToString(), song.Metadata.Name);

                        try {
                            var playlist = await _commandUtils.GetPlaylistAsync(data);

                            playlist.Songs.Add(song.Id);

                            await _musicService.Playlist.UpdateAsync(playlist);
                            await tc.SendMessageAsync($"{Format.Bold("Encoded & Installed :")} ({song.Id.ToString()}) {song.Metadata.Name}");
                        } catch (ArgumentException ex) {
                            await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "Music Manager", "Missing Playlist", ex));
                        } catch (Exception ex) {
                            await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "Music Manager", "Missing TC", ex));
                        }

                        await _log.LogToBotLogAsync(logOutput.ToString(), BotLogType.AudioChord);
                    }
                    break;
            }
        }
    }
}