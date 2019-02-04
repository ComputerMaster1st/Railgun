using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using Railgun.Core.Managers;
using Railgun.Core.Utilities;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicAdd
        {
            [Alias("youtube", "yt")]
            public class MusicAddYoutube : SystemBase
            {
                private readonly CommandUtils _commandUtils;
                private readonly MusicManager _musicManager;
                private readonly MusicService _musicService;

                public MusicAddYoutube(CommandUtils commandUtils, MusicManager musicManager, MusicService musicService) {
                    _commandUtils = commandUtils;
                    _musicManager = musicManager;
                    _musicService = musicService;
                }
                
                [Command("video")]
                public Task AddVideoAsync([Remainder] string urls) {
                    var urlList = new List<string>();

                    foreach (var url in urls.Split(new char[] { ' ', ',' })) if (!string.IsNullOrWhiteSpace(url))
                        urlList.Add(url.Trim(' ', '<', '>'));
                    
                    return Task.Factory.StartNew(async () => await _musicManager.AddYoutubeSongsAsync(urlList, (ITextChannel)Context.Channel));
                }
                
                [Command("playlist"), UserPerms(GuildPermission.ManageGuild)]
                public async Task AddPlaylistAsync(string url) {
                    var data = await Context.Database.ServerMusics.GetOrCreateAsync(Context.Guild.Id);
                    var playlist = await _commandUtils.GetPlaylistAsync(data);
                    
                    await Context.Database.SaveChangesAsync();
                    
                    var reporter = new Progress<SongProcessStatus>(async (status) => await _musicManager.YoutubePlaylistStatusUpdatedAsync((ITextChannel)Context.Channel, status, data));
                    var resolvingPlaylist = await _musicService.Youtube.DownloadPlaylistAsync(new Uri(url.Trim(' ', '<', '>')), reporter, CancellationToken.None);
                    var queued = resolvingPlaylist.Songs.Count - resolvingPlaylist.ExistingSongs;
                    var output = new StringBuilder()
                        .AppendFormat("Found In Repository : {0}",Format.Bold(resolvingPlaylist.ExistingSongs.ToString()));
                    
                    if (queued > 0) output.AppendFormat(" {0} Queued For Installation : {1}", Response.GetSeparator(), Format.Bold(queued.ToString())).AppendLine();
                    
                    output.AppendLine("Processing of YouTube Playlists may take some time... Just to let you know.");
                    
                    await ReplyAsync(output.ToString());
                    await Task.Factory.StartNew(async () => await _musicManager.ProcessYoutubePlaylistAsync(playlist, resolvingPlaylist, (ITextChannel)Context.Channel));
                }
            }
        }
    }
}