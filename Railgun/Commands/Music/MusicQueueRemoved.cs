using AudioChord;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Music;
using System.Linq;
using System.Threading.Tasks;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicQueue
        {
			[Alias("remove"), UserPerms(GuildPermission.ManageMessages)]
            public class MusicQueueRemoved : SystemBase
            {
				private readonly PlayerController _players;

				public MusicQueueRemoved(PlayerController players)
					=> _players = players;

				[Command]
				public async Task ExecuteAsync(string songIdRaw)
				{
					var playerContainer = _players.GetPlayer(Context.Guild.Id);

					if (playerContainer == null)
					{
						await ReplyAsync("There is no music player active at this time.");
						return;
					}

					var songId = SongId.Parse(songIdRaw);
					var player = playerContainer.Player;
					var request = player.MusicScheduler.Requests.FirstOrDefault(f => f.Id.ToString() == songId.ToString());

					if (request == null)
					{
						await ReplyAsync("Specified song is not in the queue.");
						return;
					}

					await player.MusicScheduler.RemoveSongRequestAsync(request);
					await ReplyAsync("Song removed from queue!");
					return;
				}
			}
        }
    }
}
