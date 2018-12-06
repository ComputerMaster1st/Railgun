using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using Railgun.Core.Logging;
using Railgun.Core.Utilities;
using TreeDiagram;

namespace Railgun.Commands.Music.MusicAdd
{
    public partial class Music
    {
        [Alias("add")]
        public partial class MusicAdd : SystemBase
        {
            private readonly TreeDiagramContext _db;
            private readonly Log _log;
            private readonly CommandUtils _commandUtils;
            private readonly MusicService _musicService;

            public MusicAdd(TreeDiagramContext db, Log log, CommandUtils commandUtils, MusicService musicService) {
                _db = db;
                _log = log;
                _commandUtils = commandUtils;
                _musicService = musicService;
            }
            
            [Command("upload")]
            public async Task UploadAsync() {
                if (Context.Message.Attachments.Count < 1) {
                    await ReplyAsync("Please specify a youtube link or upload a file.");

                    return;
                }
                
                var data = await _db.ServerMusics.GetOrCreateAsync(Context.Guild.Id);
                var playlist = await _commandUtils.GetPlaylistAsync(data);
                var response = await ReplyAsync("Processing Attachment! Standby...");
                var attachment = Context.Message.Attachments.FirstOrDefault();
                
                try {
                    var song = await _musicService.Discord.DownloadAsync(attachment.Url, $"{Context.Author.Username}#{Context.Author.DiscriminatorValue}", attachment.Id);
                    
                    playlist.Songs.Add(song.Id);
                    
                    await _musicService.Playlist.UpdateAsync(playlist);
                    await response.ModifyAsync(c => c.Content = $"Installed To Playlist - {Format.Bold(song.Metadata.Name)} || ID : {Format.Bold(song.Id.ToString())}");
                } catch (Exception ex) {
                    await response.ModifyAsync(c => c.Content = $"Install Failure - {Format.Bold("(Attached File)")} {ex.Message}");
                    
                    var output = new StringBuilder()
                        .AppendFormat("<{0} ({1})> Upload From Discord Failure!", Context.Guild.Name, Context.Guild.Id).AppendLine()
                        .AppendLine(ex.ToString());
                    
                    await _log.LogToBotLogAsync(output.ToString(), BotLogType.MusicManager);
                }
            }
            
            [Command("repo"), UserPerms(GuildPermission.ManageGuild)]
            public async Task ImportRepoAsync() {
                var data = await _db.ServerMusics.GetOrCreateAsync(Context.Guild.Id);
                var playlist = await _commandUtils.GetPlaylistAsync(data);
                var repo = (await _musicService.GetAllSongsAsync()).ToList();
                var existingSongs = 0;
                
                foreach (var song in repo) {
                    if (playlist.Songs.Contains(song.Id)) existingSongs++;
                    else playlist.Songs.Add(song.Id);
                }
                
                await _musicService.Playlist.UpdateAsync(playlist);
                await ReplyAsync($"Processing Completed! || Accepted : {Format.Bold((repo.Count() - existingSongs).ToString())} || Already Installed : {Format.Bold(existingSongs.ToString())}");
            }
        }
    }
}