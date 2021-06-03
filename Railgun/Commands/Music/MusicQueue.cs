using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Extensions;
using Railgun.Music;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        [Alias("queue")]
        public partial class MusicQueue : SystemBase
        {
			private readonly PlayerController _players;

            public MusicQueue(PlayerController playerController)
				=> _players = playerController;

            [Command, BotPerms(ChannelPermission.AttachFiles)]
			public Task ExecuteAsync()
			{
				var playerContainer = _players.GetPlayer(Context.Guild.Id);

				if (playerContainer == null)
					return ReplyAsync("I'm not playing anything at this time.");

				var player = playerContainer.Player;

				if (!player.AutoSkipped && !player.MusicScheduler.IsRequestsPopulated)
					return ReplyAsync("There are currently no music requests in the queue.");

				var i = 0;
				var output = new StringBuilder()
					.AppendFormat(Format.Bold("Queued Music Requests ({0}) :"), player.MusicScheduler.Requests.Count).AppendLine()
					.AppendLine();

				var currentTime = DateTime.Now - player.SongStartedAt;

				output.AppendFormat("Now : {0} {1} Length : {2}/{3} {1} ID: {4}",
									Format.Bold(player.CurrentSong.Metadata.Title),
									SystemUtilities.GetSeparator,
									Format.Bold($"{currentTime.Minutes}:{currentTime.Seconds}"),
												Format.Bold($"{player.CurrentSong.Metadata.Duration.Minutes}:{player.CurrentSong.Metadata.Duration.Seconds}"),
												Format.Bold(player.CurrentSong.Metadata.Id.ToString()))
								.AppendLine();

				while (player.MusicScheduler.Requests.Count > i)
				{
					var song = player.MusicScheduler.Requests[i];

					switch (i)
					{
						case 0:
							output.AppendFormat("Next : {0} {1} Length : {2} {1} ID: {3}",
												Format.Bold(song.Name),
												SystemUtilities.GetSeparator,
												Format.Bold(song.Length.ToString()),
												Format.Bold(song.Id.ToString()));
							break;
						default:
							output.AppendFormat("{0} : {1} {2} Length : {3} {1} ID: {3}",
												Format.Code($"[{i}]"),
												Format.Bold(song.Name),
												SystemUtilities.GetSeparator,
												Format.Bold(song.Length.ToString()),
												Format.Bold(song.Id.ToString()));
							break;
					}

					output.AppendLine();
					i++;
				}

				if (output.Length > 1950)
					return (Context.Channel as ITextChannel).SendStringAsFileAsync("Queue.txt", output.ToString(), $"Queued Music Requests ({player.MusicScheduler.Requests.Count})");
				return ReplyAsync(output.ToString());
			}
		}
    }
}
