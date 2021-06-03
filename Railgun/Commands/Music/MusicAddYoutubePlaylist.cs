using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Music;
using System.Threading.Tasks;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicAdd
        {
            public partial class MusicAddYoutube
            {
                [Alias("playlist"), UserPerms(GuildPermission.ManageGuild)]
                public class MusicAddYoutubePlaylist : SystemBase
                {
                    private readonly MusicController _musicController;

                    public MusicAddYoutubePlaylist(MusicController musicController)
                        => _musicController = musicController;

                    [Command]
                    public Task ExecuteAsync(string url)
                        => Task.Factory.StartNew(async () => await _musicController.ProcessYoutubePlaylistAsync(url.Trim(' ', '<', '>'), Context.Channel as ITextChannel));
                }
            }
        }
    }
}
