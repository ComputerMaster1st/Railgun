using AudioChord;
using Discord;
using Finite.Commands;
using MongoDB.Bson;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Extensions;
using System;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicPlaylist
        {
            [Alias("export"), BotPerms(ChannelPermission.AttachFiles), UserPerms(GuildPermission.ManageGuild)]
            public class MusicPlaylistExport : SystemBase
            {
                private readonly MusicService _musicService;

                public MusicPlaylistExport(MusicService musicService)
                    => _musicService = musicService;

                [Command]
                public async Task ExecuteAsync()
                {
                    var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                    var data = profile.Music;

                    if (data == null || data.PlaylistId == ObjectId.Empty)
                    {
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

                    foreach (var song in playlist.Songs)
                    {
                        output.AppendLine(song.ToString());
                    }

                    await (Context.Channel as ITextChannel).SendStringAsFileAsync($"playlist-data{SystemUtilities.FileExtension}", output.ToString());
                    await response.DeleteAsync();
                }
            }
        }
    }
}
