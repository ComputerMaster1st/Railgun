using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Music;
using System.Threading.Tasks;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        [Alias("ping")]
        public class MusicPing : SystemBase
        {
            private readonly PlayerController _players;

            public MusicPing(PlayerController players)
                => _players = players;

            [Command]
            public Task ExecuteAsync()
            {
                var container = _players.GetPlayer(Context.Guild.Id);

                return ReplyAsync(container == null ? "Can not check ping due to not being in voice channel." : $"Ping to Discord Voice: {Format.Bold(container.Player.Latency.ToString())}ms");
            }
        }
    }
}
