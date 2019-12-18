using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Finite.Commands;
using MongoDB.Bson;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Extensions;
using Railgun.Music;
using TreeDiagram;

namespace Railgun.Commands.Music 
{
    public partial class Music
    {
        [Alias("playlist")]
        public class MusicPlaylist : SystemBase
        {
            private readonly MusicService _musicService;
            private readonly MusicController _musicController;

            public MusicPlaylist(MusicService musicService, MusicController musicController) {
                _musicService = musicService;
                _musicController = musicController;
            }

            [Command, BotPerms(ChannelPermission.AttachFiles)]
            public async Task PlaylistAsync() {
                var data = Context.Database.ServerMusics.GetData(Context.Guild.Id);

                if (data == null || data.PlaylistId == ObjectId.Empty) {
                    await ReplyAsync("Server playlist == currently empty.");
                    return;
                }

                var playlist = await _musicService.Playlist.GetPlaylistAsync(data.PlaylistId);

                if (playlist == null || playlist.Songs.Count < 1) {
                    await ReplyAsync("Server playlist == currently empty.");
                    return;
                }

                var response = await ReplyAsync("Generating playlist file, standby...");
                var removedSongs = new List<SongId>();
                var output = new StringBuilder()
                    .AppendFormat("{0} Music Playlist!", Context.Guild.Name).AppendLine()
                    .AppendFormat("Total Songs : {0}", playlist.Songs.Count).AppendLine()
                    .AppendLine();

                foreach (var songId in playlist.Songs) {
                    var song = await _musicService.TryGetSongAsync(songId);

                    if (!song.Item1) {
                        removedSongs.Add(songId);
                        continue;
                    }

                    output.AppendFormat("--       Id => {0}", song.Item2.Id.ToString()).AppendLine()
                        .AppendFormat("--     Name => {0}", song.Item2.Metadata.Name).AppendLine()
                        .AppendFormat("--   Length => {0}", song.Item2.Metadata.Length).AppendLine()
                        .AppendFormat("--      Url => {0}", song.Item2.Metadata.Url).AppendLine()
                        .AppendFormat("-- Uploader => {0}", song.Item2.Metadata.Uploader).AppendLine()
                        .AppendLine();
                }

                output.AppendLine("End of Playlist.");

                if (removedSongs.Count > 0) {
                    foreach (var songId in removedSongs) playlist.Songs.Remove(songId);
                    await _musicService.Playlist.UpdateAsync(playlist);
                }

                await (Context.Channel as ITextChannel).SendStringAsFileAsync("Playlist.txt", output.ToString(), $"{Context.Guild.Name} Music Playlist ({playlist.Songs.Count} songs)");
                await response.DeleteAsync();
            }

            [Command("export"), BotPerms(ChannelPermission.AttachFiles), UserPerms(GuildPermission.ManageGuild)]
            public async Task ExportAsync() {
                var data = Context.Database.ServerMusics.GetData(Context.Guild.Id);

                if (data == null || data.PlaylistId == ObjectId.Empty) {
                    await ReplyAsync("There's no playlist data to export.");
                    return;
                }

                var response = await ReplyAsync("Building Playlist data file. Standby...");
                var output = new StringBuilder()
                    .AppendFormat("# {0}'s Playlist.", Context.Guild.Name).AppendLine()
                    .AppendFormat("# Generated At : {0}", DateTime.Now).AppendLine()
                    .AppendLine("#")
                    .AppendLine("# !!! DO NOT CHANGE/MODIFY THIS FILE !!! ")
                    .AppendLine();
                
                var playlist = await SystemUtilities.GetPlaylistAsync(_musicService, data);

                foreach (var song in playlist.Songs) {
                    if (song.ProcessorId != "YOUTUBE") continue;
                    output.AppendLine(song.ToString());
                }

                await (Context.Channel as ITextChannel).SendStringAsFileAsync($"playlist-data{SystemUtilities.FileExtension}", output.ToString());
                await response.DeleteAsync();
            }

            [Command("import"), UserPerms(GuildPermission.ManageGuild)]
            public async Task ImportAsync() {
                var data = Context.Database.ServerMusics.GetOrCreateData(Context.Guild.Id);
                var playlist = await SystemUtilities.GetPlaylistAsync(_musicService, data);

                if (Context.Message.Attachments.Count < 1) {
                    await ReplyAsync("Please attach the playlist export data file.");
                    return;
                }

                var response = await ReplyAsync("Processing playlist data, standby...");
                var importFileUrl = Context.Message.Attachments.First().Url;
                var importFileName = Context.Guild.Id + $"-playlist-data{SystemUtilities.FileExtension}";
                
                if (File.Exists(importFileName)) File.Delete(importFileName);

                using (var webClient = new HttpClient())
                using (var writer = File.OpenWrite(importFileName)) {
                    var importStream = await webClient.GetStreamAsync(importFileUrl);
                    await importStream.CopyToAsync(writer);
                }

                var importFile = await File.ReadAllLinesAsync(importFileName);
                var idList = new List<string>();

                foreach (var line in importFile) {
                    if (line.StartsWith('#') || string.IsNullOrWhiteSpace(line)) continue;

                    var songId = SongId.Parse(line);
                    idList.Add($"https://youtu.be/{songId.SourceId}");
                }

                File.Delete(importFileName);

                await response.ModifyAsync(x => x.Content = $"Discovered {Format.Bold(idList.Count.ToString())} IDs! Beginning Import... Please note, this may take a while depending on how many songs there are.");
                await Task.Factory.StartNew(async () => await _musicController.AddYoutubeSongsAsync(idList, Context.Channel as ITextChannel));
            }
        }
    }
}