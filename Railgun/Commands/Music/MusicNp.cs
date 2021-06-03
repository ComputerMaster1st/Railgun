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
		public partial class MusicNp : SystemBase
		{
			private readonly PlayerController _playerController;

			public MusicNp(PlayerController playerController) => _playerController = playerController;

			[Command]
			public Task NowPlayingAsync()
			{
				var container = _playerController.GetPlayer(Context.Guild.Id);

				if (container == null) return ReplyAsync("I'm not playing anything at this time.");

				var player = container.Player;
				var meta = player.CurrentSong.Metadata;
				var currentTime = DateTime.Now - player.SongStartedAt;
				var output = new StringBuilder()
					.AppendFormat("Currently playing {0} at the moment.", Format.Bold(meta.Title)).AppendLine()
					.AppendFormat("Url: {0} {1} Length: {2}/{3}", Format.Bold($"<{meta.Source}>"),
								  SystemUtilities.GetSeparator,
								  Format.Bold($"{currentTime.Minutes}:{currentTime.Seconds}"),
								  Format.Bold($"{meta.Duration.Minutes}:{meta.Duration.Seconds}"));

				return ReplyAsync(output.ToString());
			}
        }
	}
}