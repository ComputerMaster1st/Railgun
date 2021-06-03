using System.Threading.Tasks;
using Finite.Commands;
using Railgun.Core;
using Railgun.Music;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        [Alias("leave")]
        public partial class MusicLeave : SystemBase
        {
            private readonly PlayerController _players;
            
            public MusicLeave(PlayerController playerController)
                => _players = playerController;
            
            [Command]
            public async Task ExecuteAsync() 
            {
                if (_players.GetPlayer(Context.Guild.Id) == null) {
                    await ReplyAsync("I'm not streaming any music at this time.");
                    return;
                }
            
                await ReplyAsync("Stopping Music Stream...");

                _players.DisconnectPlayer(Context.Guild.Id);
            }
        }
    }
}