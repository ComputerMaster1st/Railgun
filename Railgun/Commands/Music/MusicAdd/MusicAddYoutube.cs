using System;
using System.Linq;
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

				public MusicAddYoutube(CommandUtils commandUtils, MusicManager musicManager, MusicService musicService)
				{
					_commandUtils = commandUtils;
					_musicManager = musicManager;
					_musicService = musicService;
				}

				[Command("video")]
				public Task AddVideoAsync([Remainder] string urls)
				{
					var urlList = (from url in urls.Split(new char[] {' ', ','}) where !string.IsNullOrWhiteSpace(url) 
						select url.Trim(' ', '<', '>')).ToList();
					return Task.Factory.StartNew(async () => await _musicManager.AddYoutubeSongsAsync(urlList, (ITextChannel)Context.Channel));
				}

				[Command("playlist"), UserPerms(GuildPermission.ManageGuild)]
				public async Task AddPlaylistAsync(string url)
				{
					var data = Context.Database.ServerMusics.GetOrCreateData(Context.Guild.Id);
					var playlist = await _commandUtils.GetPlaylistAsync(data);
					var cleanUrl = url.Trim(' ', '<', '>');

					await Context.Database.SaveChangesAsync();

					async void Handler(SongProcessStatus status) => await _musicManager.YoutubePlaylistStatusUpdatedAsync((ITextChannel)Context.Channel, status, data);
					var reporter = new Progress<SongProcessStatus>(Handler);
					var resolvingPlaylist = await _musicService.Youtube.DownloadPlaylistAsync(new Uri(cleanUrl), reporter, CancellationToken.None);
					var queued = resolvingPlaylist.Songs.Count - resolvingPlaylist.ExistingSongs;
					var output = new StringBuilder()
						.AppendFormat("Found In Repository : {0}", Format.Bold(resolvingPlaylist.ExistingSongs.ToString()));

					if (queued > 0) output.AppendFormat(" {0} Queued For Installation : {1}", Response.GetSeparator(), Format.Bold(queued.ToString())).AppendLine();

					output.AppendLine("Processing of YouTube Playlists may take some time... Just to let you know.");

					await ReplyAsync(output.ToString());
					await Task.Factory.StartNew(async () => await _musicManager.ProcessYoutubePlaylistAsync(cleanUrl, playlist, resolvingPlaylist, (ITextChannel)Context.Channel));
				}
			}
		}
	}
}