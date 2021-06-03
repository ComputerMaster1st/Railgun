using System.Linq;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Music;

namespace Railgun.Commands.Music
{
    public partial class Music
	{
		public partial class MusicAdd
		{
			[Alias("youtube", "yt")]
			public partial class MusicAddYoutube : SystemBase
			{
				private readonly MusicController _musicController;

                public MusicAddYoutube(MusicController musicController) => _musicController = musicController;

                [Command("video")]
				public Task AddVideoAsync([Remainder] string urls)
				{
					var urlList = (from url in urls.Split(new char[] {' ', ','}) where !string.IsNullOrWhiteSpace(url) 
						select url.Trim(' ', '<', '>')).ToList();
					return Task.Factory.StartNew(async () => await _musicController.AddYoutubeSongsAsync(urlList, (ITextChannel)Context.Channel));
				}
            }
		}
	}
}