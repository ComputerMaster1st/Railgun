using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using Railgun.Core.Managers;
using Railgun.Core.Utilities;
using TreeDiagram;

namespace Railgun.Commands.Music
{
	public partial class Music
	{
		[Alias("skip")]
		public class MusicSkip : SystemBase
		{
			private readonly CommandUtils _commandUtils;
			private readonly PlayerManager _playerManager;

			public MusicSkip(CommandUtils commandUtils, PlayerManager playerManager)
			{
				_commandUtils = commandUtils;
				_playerManager = playerManager;
			}

			[Command]
			public async Task SkipAsync()
			{
				var data = Context.Database.ServerMusics.GetData(Context.Guild.Id);
				var container = _playerManager.GetPlayer(Context.Guild.Id);

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
					var name = _commandUtils.GetUsernameOrMention((IGuildUser)Context.Author);

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
				var container = _playerManager.GetPlayer(Context.Guild.Id);

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