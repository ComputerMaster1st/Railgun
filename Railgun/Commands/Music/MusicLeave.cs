using System.Threading.Tasks;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Managers;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        [Alias("leave")]
        public class MusicLeave : SystemBase
        {
            private readonly PlayerManager _playerManager;
            
            public MusicLeave(PlayerManager playerManager) => _playerManager = playerManager;
            
            [Command]
            public async Task LeaveAsync() {
                if (!_playerManager.IsCreated(Context.Guild.Id)) {
                    await ReplyAsync("I'm not streaming any music at this time.");

                    return;
                }
            
                await ReplyAsync("Stopping Music Stream...");

                _playerManager.DisconnectPlayer(Context.Guild.Id);
            }
            
            [Command("aftersong")]
            public async Task LeaveAfterSongAsync() {
                var container = _playerManager.GetPlayer(Context.Guild.Id);
                
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