using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Music;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    public partial class Music
	{
		[Alias("skip")]
		public partial class MusicSkip : SystemBase
		{
			private readonly PlayerController _players;

			public MusicSkip(PlayerController playerManager)
				=> _players = playerManager;

			[Command]
			public async Task ExecuteAsync()
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Music;
				var container = _players.GetPlayer(Context.Guild.Id);

				if (container == null) {
					await ReplyAsync("Can not skip current song because I am not in voice channel.");
					return;
				}
				if (!data.VoteSkipEnabled) {
					container.Player.SkipMusic();
					await ReplyAsync("Skipping music now...");
					return;
				}

				var player = container.Player;
				var userCount = await player.GetUserCountAsync();
				var voteSkipResult = player.VoteSkip(Context.Author.Id);
				var percent = player.VoteSkipped.Count / userCount * 100;

				if (percent < data.VoteSkipLimit) {
					var name = SystemUtilities.GetUsernameOrMention(Context.Database, (IGuildUser)Context.Author);
					await ReplyAsync($"{Format.Bold(name)} has voted to skip the current song!");
					return;
				} else if (!voteSkipResult) {
					await ReplyAsync("You've already voted to skip!");
					return;
				}

				player.SkipMusic();

				await ReplyAsync("Vote-Skipping music now...");
			}
		}
	}
}