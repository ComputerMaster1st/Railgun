using System.Linq;
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
					var cleanUrl = url.Trim(' ', '<', '>');
                    await _musicController.ProcessYoutubePlaylistAsync(cleanUrl, Context.Channel as ITextChannel);
				}
			}
		}
	}
}