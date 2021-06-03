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
            private readonly PlayerController _playerController;
            
            public MusicLeave(PlayerController playerController) => _playerController = playerController;
            
            [Command]
            public async Task LeaveAsync() {
                if (_playerController.GetPlayer(Context.Guild.Id) == null) {
                    await ReplyAsync("I'm not streaming any music at this time.");
                    return;
                }
            
                await ReplyAsync("Stopping Music Stream...");
                _playerController.DisconnectPlayer(Context.Guild.Id);
            }
        }
    }
}