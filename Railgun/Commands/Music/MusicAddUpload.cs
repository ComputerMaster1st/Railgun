using AudioChord;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Enums;
using Railgun.Music;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicAdd
        {
			[Alias("upload")]
            public class MusicAddUpload : SystemBase
            {
				private readonly MusicService _musicService;
				private readonly MetaDataEnricher _enricher;
				private readonly BotLog _botLog;

				public MusicAddUpload(MusicService musicService, MetaDataEnricher enricher, BotLog botLog)
                {
					_musicService = musicService;
					_enricher = enricher;
					_botLog = botLog;
                }

				[Command]
				public async Task ExecuteAsync()
				{
					if (Context.Message.Attachments.Count < 1)
					{
						await ReplyAsync("Please specify a youtube link or upload a file.");
						return;
					}

					var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
					var data = profile.Music;
					var playlist = await SystemUtilities.GetPlaylistAsync(_musicService, data);
					var response = await ReplyAsync("Processing Attachment! Standby...");
					var attachment = Context.Message.Attachments.FirstOrDefault();

					if (attachment == null)
					{
						await ReplyAsync("Please specify a music file.");
						return;
					}

					try
					{
						_enricher.AddMapping($"{Context.Author.Username}#{Context.Author.DiscriminatorValue}", SongId.Parse($"DISCORD#{attachment.Id}"), attachment.Filename);
						var song = await _musicService.DownloadSongAsync(attachment.Url);

						playlist.Songs.Add(song.Metadata.Id);

						await SystemUtilities.UpdatePlaylistAsync(_musicService, playlist);
						await response.ModifyAsync(c => c.Content = $"Installed To Playlist - {Format.Bold(song.Metadata.Title)} {SystemUtilities.GetSeparator} ID : {Format.Bold(song.Metadata.Id.ToString())}");
					}
					catch (Exception ex)
					{
						await response.ModifyAsync(c => c.Content = $"Install Failure - {Format.Bold("(Attached File)")} {ex.Message}");

						var output = new StringBuilder()
							.AppendFormat("<{0} ({1})> Upload From Discord Failure!", Context.Guild.Name, Context.Guild.Id).AppendLine()
							.AppendLine(ex.ToString());

						await _botLog.SendBotLogAsync(BotLogType.MusicManager, output.ToString());
					}
				}
			}
        }
    }
}
