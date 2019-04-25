using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Music;
using TreeDiagram;

namespace Railgun.Commands.Music
{
	public partial class Music
	{
		[Alias("skip")]
		public class MusicSkip : SystemBase
		{
			private readonly PlayerController _playerController;

			public MusicSkip(PlayerController playerManager) => _playerController = playerManager;

			[Command]
			public async Task SkipAsync()
			{
				var data = Context.Database.ServerMusics.GetData(Context.Guild.Id);
				var container = _playerController.GetPlayer(Context.Guild.Id);

				if (data == null || container == null) {
					await ReplyAsync("Can not skip current song because I am not in voice channel.");

					return;
				} else if (!data.VoteSkipEnabled) {
					await ReplyAsync("Skipping music now...");

					container.Player.CancelMusic();
					return;
				}

				var player = container.Player;
				var userCount = player.GetUserCount();
				var voteSkipResult = player.VoteSkip(Context.Author.Id);
				var percent = (player.VoteSkipped.Count / userCount) * 100;

				if (percent < data.VoteSkipLimit) {
					var name = SystemUtilities.GetUsernameOrMention(Context.Database, (IGuildUser)Context.Author);

					await ReplyAsync($"{Format.Bold(name)} has voted to skip the current song!");

					return;
				} else if (!voteSkipResult) {
					await ReplyAsync("You've already voted to skip!");

					return;
				}

				await ReplyAsync("Vote-Skipping music now...");

				player.CancelMusic();
			}

			[Command("force"), UserPerms(GuildPermission.ManageGuild)]
			public async Task ForceAsync()
			{
				var data = Context.Database.ServerMusics.GetData(Context.Guild.Id);
				var container = _playerController.GetPlayer(Context.Guild.Id);

				if (data == null || !data.VoteSkipEnabled) {
					await ReplyAsync("This command is not available due to Music Vote-Skip being disabled.");

					return;
				} else if (container == null) {
					await ReplyAsync("Can not skip current song because I am not in voice channel.");

					return;
				}

				await ReplyAsync("Force-Skipping music now...");

				container.Player.CancelMusic();
			}
		}
	}
}