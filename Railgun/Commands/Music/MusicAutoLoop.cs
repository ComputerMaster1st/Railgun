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
            [Alias("loop")]
            public class MusicAutoLoop : SystemBase
            {
                private readonly PlayerController _players;

                public MusicAutoLoop(PlayerController players)
                    => _players = players;

                [Command]
                public Task ExecuteAsync()
                {
                    var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                    var data = profile.Music;

                    data.PlaylistAutoLoop = !data.PlaylistAutoLoop;

                    var container = _players.GetPlayer(Context.Guild.Id);

                    if (container != null) 
                        container.Player.MusicScheduler.PlaylistAutoLoop = data.PlaylistAutoLoop;

                    return ReplyAsync($"Music Playlist Auto-Loop is now {Format.Bold(data.PlaylistAutoLoop ? "Enabled" : "Disabled")}.");
                }
            }
        }
    }
}
