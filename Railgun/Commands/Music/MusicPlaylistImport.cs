using AudioChord;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Music;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
                private readonly MusicController _musicController;

                public MusicPlaylistImport(MusicService musicService, MusicController musicController)
                {
                    _musicService = musicService;
                    _musicController = musicController;
                }

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

                    foreach (var line in importFile)
                    {
                        if (line.StartsWith('#') || string.IsNullOrWhiteSpace(line)) 
                            continue;

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
}
