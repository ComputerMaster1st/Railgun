using Finite.Commands;
using Railgun.Core;
using Railgun.Music;
using System.Threading.Tasks;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicRoot
        {
            [Alias("kill")]
            public class MusicRootKill : SystemBase
            {
                private readonly PlayerController _players;

                public MusicRootKill(PlayerController players)
                    => _players = players;

                [Command]
                public Task ExecuteAsync(ulong id)
                {
                    if (_players.DisconnectPlayer(id)) 
                        return ReplyAsync($"Sent 'Kill Code' to Player ID {id}.");

                    return ReplyAsync($"No player found with ID {id}.");
                }
            }
        }
    }
}
