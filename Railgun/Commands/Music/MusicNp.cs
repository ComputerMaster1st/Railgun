using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Music;

namespace Railgun.Commands.Music
{
    public partial class Music
	{
		[Alias("np", "nowplaying", "playing")]
		public partial class MusicNp : SystemBase
		{
			private readonly PlayerController _players;

			public MusicNp(PlayerController playerController)
				=> _players = playerController;

			[Command]
			public Task ExecuteAsync()
			{
				var container = _players.GetPlayer(Context.Guild.Id);

				if (container == null) 
					return ReplyAsync("I'm not playing anything at this time.");

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