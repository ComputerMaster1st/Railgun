using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Music;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Commands.Music
{
	public partial class Music
	{
		[Alias("np")]
		public class MusicNp : SystemBase
		{
			private readonly PlayerController _playerController;

			public MusicNp(PlayerController playerController) => _playerController = playerController;

			private Task SetNpChannelAsync(ServerMusic data, ITextChannel tc, bool locked = false)
			{
				data.NowPlayingChannel = locked ? tc.Id : 0;
				return ReplyAsync($"{Format.Bold("Now Playing")} messages are {Format.Bold(locked ? "Now" : "No Longer")} locked to #{tc.Name}.");
			}

			[Command]
			public Task NowPlayingAsync()
			{
				var container = _playerController.GetPlayer(Context.Guild.Id);

				if (container == null) return ReplyAsync("I'm not playing anything at this time.");

				var player = container.Player;
				var meta = player.CurrentSong.Metadata;
				var currentTime = DateTime.Now - player.SongStartedAt;
				var output = new StringBuilder()
					.AppendFormat("Currently playing {0} at the moment.", Format.Bold(meta.Name)).AppendLine()
					.AppendFormat("Url: {0} {1} Length: {2}/{3}", Format.Bold($"<{meta.Url}>"),
								  SystemUtilities.GetSeparator,
								  Format.Bold($"{currentTime.Minutes}:{currentTime.Seconds}"),
								  Format.Bold($"{meta.Length.Minutes}:{meta.Length.Seconds}"));

				return ReplyAsync(output.ToString());
			}

			[Command("channel"), UserPerms(GuildPermission.ManageGuild)]
			public Task SetNpChannelAsync(ITextChannel tcParam = null)
			{
				var data = Context.Database.ServerMusics.GetOrCreateData(Context.Guild.Id);
				var tc = tcParam ?? Context.Channel as ITextChannel;

				if (data.NowPlayingChannel != 0 && tc.Id == data.NowPlayingChannel)
					return SetNpChannelAsync(data, tc);
				return SetNpChannelAsync(data, tc, true);
			}
		}
	}
}