using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Music;
using System.Linq;
using System.Threading.Tasks;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicAdd
        {
            public partial class MusicAddYoutube
            {
                [Alias("video")]
                public class MusicAddYoutubeVideo : SystemBase
                {
                    private readonly MusicController _musicController;

                    public MusicAddYoutubeVideo(MusicController musicController)
                        => _musicController = musicController;

                    [Command]
                    public Task ExecuteAsync([Remainder] string urls)
                    {
                        var urlList = (from url in urls.Split(new char[] { ' ', ',' })
                                       where !string.IsNullOrWhiteSpace(url)
                                       select url.Trim(' ', '<', '>')).ToList();

                        return Task.Factory.StartNew(async () => await _musicController.AddYoutubeSongsAsync(urlList, (ITextChannel)Context.Channel));
                    }
                }
            }
        }
    }
}
