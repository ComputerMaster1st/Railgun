using Finite.Commands;
using Railgun.Core;
using Railgun.Music;
using System.Threading.Tasks;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicLeave
        {
            [Alias("aftersong")]
            public class MusicLeaveAftersong : SystemBase
            {
                private readonly PlayerController _players;

                public MusicLeaveAftersong(PlayerController players)
                    => _players = players;

                [Command]
                public Task ExecuteAsync()
                {
                    var container = _players.GetPlayer(Context.Guild.Id);

                    if (container == null)
                        return ReplyAsync("I'm not streaming any music at this time.");

                    container.Player.LeaveAfterSong = true;

                    return ReplyAsync("I shall leave after this song has finished playing.");
                }
            }
        }
    }
}
