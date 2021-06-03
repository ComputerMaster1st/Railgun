using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Music;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicAuto
        {
            [Alias("shuffle")]
            public class MusicAutoShuffle : SystemBase
            {
                private readonly PlayerController _players;

                public MusicAutoShuffle(PlayerController players)
                    => _players = players;

                [Command]
                public Task ExecuteAsync()
                {
                    var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                    var data = profile.Music;

                    data.DisableShuffle = !data.DisableShuffle;

                    var container = _players.GetPlayer(Context.Guild.Id);

                    if (container != null) 
                        container.Player.MusicScheduler.DisableShuffle = data.DisableShuffle;

                    return ReplyAsync($"Music Playlist Shuffle is now {Format.Bold(data.DisableShuffle ? "Disabled" : "Enabled")}.");
                }
            }
        }
    }
}
