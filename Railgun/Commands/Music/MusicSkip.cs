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
			public Task SkipAsync()
			{
				var data = Context.Database.ServerMusics.GetData(Context.Guild.Id);
				var container = _playerController.GetPlayer(Context.Guild.Id);

				if (data == null || container == null) 
					return ReplyAsync("Can not skip current song because I am not in voice channel.");
				if (!data.VoteSkipEnabled) {
					container.Player.CancelMusic();
					return ReplyAsync("Skipping music now...");
				}

				var player = container.Player;
				var userCount = player.GetUserCount();
				var voteSkipResult = player.VoteSkip(Context.Author.Id);
				var percent = (player.VoteSkipped.Count / userCount) * 100;

				if (percent < data.VoteSkipLimit) {
					var name = SystemUtilities.GetUsernameOrMention(Context.Database, (IGuildUser)Context.Author);
					return ReplyAsync($"{Format.Bold(name)} has voted to skip the current song!");
				} else if (!voteSkipResult)
					return ReplyAsync("You've already voted to skip!");

				player.CancelMusic();
				return ReplyAsync("Vote-Skipping music now...");
			}

			[Command("force"), UserPerms(GuildPermission.ManageGuild)]
			public Task ForceAsync()
			{
				var data = Context.Database.ServerMusics.GetData(Context.Guild.Id);
				var container = _playerController.GetPlayer(Context.Guild.Id);

				if (data == null || !data.VoteSkipEnabled) 
					return ReplyAsync("This command is not available due to Music Vote-Skip being disabled.");
				if (container == null)
					return ReplyAsync("Can not skip current song because I am not in voice channel.");

				container.Player.CancelMusic();
				return ReplyAsync("Force-Skipping music now...");
			}
		}
	}
}