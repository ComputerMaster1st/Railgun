using System.Threading.Tasks;
using Finite.Commands;
using Railgun.Core;
using Railgun.Music;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        [Alias("leave")]
        public class MusicLeave : SystemBase
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
            
            [Command("aftersong")]
            public async Task LeaveAfterSongAsync() {
                var container = _playerController.GetPlayer(Context.Guild.Id);
                
                if (container == null) {
                    await ReplyAsync("I'm not streaming any music at this time.");

                    return;
                }
                
                container.Player.LeaveAfterSong = true;

                await ReplyAsync("I shall leave after this song has finished playing.");
            }
        }
    }
}