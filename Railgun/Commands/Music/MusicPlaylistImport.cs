using AudioChord;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicPlaylist
        {
            [Alias("import"), UserPerms(GuildPermission.ManageGuild)]
            public class MusicPlaylistImport : SystemBase
            {
                private readonly MusicService _musicService;

                public MusicPlaylistImport(MusicService musicService)
                    => _musicService = musicService;

                [Command]
                public async Task ExecuteAsync()
                {
                    var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                    var data = profile.Music;
                    var playlist = await SystemUtilities.GetPlaylistAsync(_musicService, data);

                    if (Context.Message.Attachments.Count < 1)
                    {
                        await ReplyAsync("Please attach the playlist export data file.");
                        return;
                    }

                    var response = await ReplyAsync("Processing playlist data, standby...");
                    var importFileUrl = Context.Message.Attachments.First().Url;
                    var importFileName = Context.Guild.Id + $"-playlist-data{SystemUtilities.FileExtension}";

                    if (File.Exists(importFileName))
                        File.Delete(importFileName);

                    using (var webClient = new HttpClient())
                    using (var writer = File.OpenWrite(importFileName))
                    {
                        var importStream = await webClient.GetStreamAsync(importFileUrl);
                        await importStream.CopyToAsync(writer);
                    }

                    var importFile = await File.ReadAllLinesAsync(importFileName);
                    var idList = new List<string>();
                    (int Success, int Failed, int Skipped) = (0, 0, 0);

                    foreach (var line in importFile)
                    {
                        if (line.StartsWith('#') || string.IsNullOrWhiteSpace(line))
                            continue;

                        try
                        {
                            var songId = SongId.Parse(line);

                            if (!playlist.Songs.Contains(songId))
                            {
                                playlist.Songs.Add(songId);

                                Success++;
                            }
                            else
                                Skipped++;
                        }
                        catch
                        {
                            Failed++;
                        }
                    }

                    await _musicService.Playlist.UpdateAsync(playlist);

                    File.Delete(importFileName);

                    var output = new StringBuilder()
                        .AppendLine("Playlist Import Completed!")
                        .AppendLine()
                        .AppendFormat("Success: {0}", Format.Bold(Success.ToString())).AppendLine()
                        .AppendFormat("Skipped: {0}", Format.Bold(Skipped.ToString())).AppendLine()
                        .AppendFormat("Failed: {0}", Format.Bold(Failed.ToString()));

                    await response.ModifyAsync(x => x.Content = output.ToString());
                }
            }
        }
    }
}
