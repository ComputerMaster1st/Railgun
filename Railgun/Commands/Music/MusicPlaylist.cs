using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Finite.Commands;
using MongoDB.Bson;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Extensions;
using TreeDiagram;

namespace Railgun.Commands.Music 
{
    public partial class Music
    {
        [Alias("playlist")]
        public class MusicPlaylist : SystemBase
        {
            private readonly MusicService _musicService;

            public MusicPlaylist(MusicService musicService) {
                _musicService = musicService;
            }

            [Command(), BotPerms(ChannelPermission.AttachFiles)]
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
                    ISong song = null;

                    if (!await _musicService.TryGetSongAsync(songId, result => song = result)) {
                        removedSongs.Add(songId);
                        continue;
                    }

                    output.AppendFormat("--       Id => {0}", song.Id.ToString()).AppendLine()
                        .AppendFormat("--     Name => {0}", song.Metadata.Name).AppendLine()
                        .AppendFormat("--   Length => {0}", song.Metadata.Length).AppendLine()
                        .AppendFormat("--      Url => {0}", song.Metadata.Url).AppendLine()
                        .AppendFormat("-- Uploader => {0}", song.Metadata.Uploader).AppendLine()
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
        }
    }
}