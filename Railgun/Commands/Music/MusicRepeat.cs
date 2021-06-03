using Finite.Commands;
using Railgun.Core;
using Railgun.Music;
using System.Threading.Tasks;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
		[Alias("repeat")]
        public class MusicRepeat : SystemBase
        {
			private readonly PlayerController _players;

			public MusicRepeat(PlayerController players)
				=> _players = players;

			[Command]
			public Task ExecuteAsync(int count)
			{
				var container = _players.GetPlayer(Context.Guild.Id);

				if (container == null)
					return ReplyAsync("I'm not playing anything at this time.");

				var player = container.Player;

				player.RepeatSong = count;

				return ReplyAsync("Repeating song after finishing.");
			}

			[Command]
			public Task ExecuteAsync()
				=> ExecuteAsync(1);
		}
    }
}
