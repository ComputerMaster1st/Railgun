using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Music;
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
				private readonly MusicController _musicController;
				private readonly MusicService _musicService;

				public MusicAddYoutube(MusicController musicController, MusicService musicService)
				{
					_musicController = musicController;
					_musicService = musicService;
				}

				[Command("video")]
				public Task AddVideoAsync([Remainder] string urls)
				{
					var urlList = (from url in urls.Split(new char[] {' ', ','}) where !string.IsNullOrWhiteSpace(url) 
						select url.Trim(' ', '<', '>')).ToList();
					return Task.Factory.StartNew(async () => await _musicController.AddYoutubeSongsAsync(urlList, (ITextChannel)Context.Channel));
				}

				[Command("playlist"), UserPerms(GuildPermission.ManageGuild)]
				public async Task AddPlaylistAsync(string url)
				{
					var data = Context.Database.ServerMusics.GetOrCreateData(Context.Guild.Id);
					var playlist = await SystemUtilities.GetPlaylistAsync(_musicService, data);
					var cleanUrl = url.Trim(' ', '<', '>');

					await Context.Database.SaveChangesAsync();

                    await new PlaylistResult(_musicService, Context.Channel as ITextChannel, data)
                        .ProcessPlaylistAsync(_musicController, cleanUrl, playlist);
				}
			}
		}
	}
}