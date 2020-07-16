using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Music;
using TreeDiagram;
using TreeDiagram.Models;
using TreeDiagram.Models.Server;

namespace Railgun.Commands.Music
{
	public partial class Music
	{
		[Alias("skip")]
		public class MusicSkip : SystemBase
		{
			private readonly PlayerController _playerController;

			public MusicSkip(PlayerController playerManager) => _playerController = playerManager;

			private ServerMusic GetData(ulong guildId, bool create = false)
			{
				ServerProfile data;

				if (create)
					data = Context.Database.ServerProfiles.GetOrCreateData(guildId);
				else {
					data = Context.Database.ServerProfiles.GetData(guildId);

					if (data == null) 
						return null;
				}

				if (data.Music == null)
					if (create)
						data.Music = new ServerMusic();
				
				return data.Music;
			}

			[Command]
			public async Task SkipAsync()
			{
				var data = GetData(Context.Guild.Id);
				var container = _playerController.GetPlayer(Context.Guild.Id);

				if (data == null || container == null) {
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

			[Command("force"), UserPerms(GuildPermission.ManageMessages)]
			public Task ForceAsync()
			{
				var data = GetData(Context.Guild.Id);
				var container = _playerController.GetPlayer(Context.Guild.Id);

				if (data == null || !data.VoteSkipEnabled) 
					return ReplyAsync("This command is not available due to Music Vote-Skip being disabled.");
				if (container == null)
					return ReplyAsync("Can not skip current song because I am not in voice channel.");

				container.Player.SkipMusic();
				return ReplyAsync("Force-Skipping music now...");
			}
		}
	}
}