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
        public partial class MusicPlaylist : SystemBase
        {
            private readonly MusicService _musicService;
            private readonly MusicController _musicController;

            public MusicPlaylist(MusicService musicService, MusicController musicController) {
                _musicService = musicService;
                _musicController = musicController;
            }

            [Command, BotPerms(ChannelPermission.AttachFiles)]
            public async Task PlaylistAsync() {
                var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Music;

                if (data == null || data.PlaylistId == ObjectId.Empty) {
                    await ReplyAsync("Server playlist == currently empty.");
                    return;
                }

                var playlist = await SystemUtilities.GetPlaylistAsync(_musicService, data);

                if (playlist == null || playlist.Songs.Count < 1) {
                    await ReplyAsync("Server playlist == currently empty.");
                    return;
                }

                var response = await ReplyAsync("Generating playlist file, standby...");
                var output = new StringBuilder()
                    .AppendFormat("{0} Music Playlist!", Context.Guild.Name).AppendLine()
                    .AppendFormat("Total Songs : {0}", playlist.Songs.Count).AppendLine()
                    .AppendLine();

                foreach (var songId in playlist.Songs) {
                    var song = await _musicService.GetSongAsync(songId);

                    if (song == null) {
                        output.AppendFormat("--       Id => {0} (ENCODING REQUIRED)", songId.ToString()).AppendLine();
                        continue;
                    }

                    output.AppendFormat("--       Id => {0}", song.Metadata.Id.ToString()).AppendLine()
                        .AppendFormat("--     Name => {0}", song.Metadata.Title).AppendLine()
                        .AppendFormat("--   Length => {0}", song.Metadata.Duration).AppendLine()
                        .AppendFormat("--      Url => {0}", song.Metadata.Source).AppendLine()
                        .AppendFormat("-- Uploader => {0}", song.Metadata.Uploader).AppendLine()
                        .AppendLine();
                }

                output.AppendLine("End of Playlist.");

                await (Context.Channel as ITextChannel).SendStringAsFileAsync("Playlist.txt", output.ToString(), $"{Context.Guild.Name} Music Playlist ({playlist.Songs.Count} songs)");
                await response.DeleteAsync();
            }
        }
    }
}