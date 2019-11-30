using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Finite.Commands;
using MongoDB.Bson;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Configuration;
using Railgun.Core.Enums;
using Railgun.Core.Extensions;
using Railgun.Music;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    [Alias("music", "m"), RoleLock(ModuleType.Music)]
	public partial class Music : SystemBase
	{
		private readonly MasterConfig _config;
		private readonly PlayerController _playerController;
		private readonly MusicService _musicService;

		public Music(MasterConfig config, PlayerController playerController, MusicService musicService)
		{
			_config = config;
			_playerController = playerController;
			_musicService = musicService;
		}

		[Command("join"), BotPerms(GuildPermission.Connect | GuildPermission.Speak)]
		public Task JoinAsync()
		{
			if (_playerController.GetPlayer(Context.Guild.Id) != null)
				return ReplyAsync($"Sorry, I'm already in a voice channel. If you're experiencing problems, please do {Format.Code($"{_config.DiscordConfig.Prefix}music reset stream.")}");

			var user = (IGuildUser)Context.Author;
			var vc = user.VoiceChannel;

			if (vc == null) return ReplyAsync("Please go into a voice channel before inviting me.");

			return _playerController.CreatePlayerAsync(user, vc, (ITextChannel)Context.Channel);
		}

        [Command("repeat")]
        public Task RepeatAsync() => RepeatAsync(1);

		[Command("repeat")]
		public Task RepeatAsync(int count)
		{
			var container = _playerController.GetPlayer(Context.Guild.Id);

			if (container == null) return ReplyAsync("I'm not playing anything at this time.");

			var player = container.Player;
			player.RepeatSong = count;

			return ReplyAsync("Repeating song after finishing.");
		}

		[Command("ping")]
		public Task PingAsync()
		{
			var container = _playerController.GetPlayer(Context.Guild.Id);
			return ReplyAsync(container == null ? "Can not check ping due to not being in voice channel." : $"Ping to Discord Voice: {Format.Bold(container.Player.Latency.ToString())}ms");
		}

		[Command("queue"), BotPerms(ChannelPermission.AttachFiles)]
		public Task QueueAsync()
		{
			var playerContainer = _playerController.GetPlayer(Context.Guild.Id);

			if (playerContainer == null)
				return ReplyAsync("I'm not playing anything at this time.");

			var player = playerContainer.Player;

			if (!player.AutoSkipped && player.Requests.Count < 1)
				return ReplyAsync("There are currently no music requests in the queue.");

			var i = 0;
			var output = new StringBuilder()
				.AppendFormat(Format.Bold("Queued Music Requests ({0}) :"), player.Requests.Count).AppendLine()
				.AppendLine();

			while (player.Requests.Count > i) {
				var song = player.Requests[i];

				switch (i) {
					case 0:
						var currentTime = DateTime.Now - player.SongStartedAt;

						output.AppendFormat("Now : {0} {1} Length : {2}/{3}",
											Format.Bold(song.Name),
											SystemUtilities.GetSeparator,
											Format.Bold($"{currentTime.Minutes}:{currentTime.Seconds}"),
											Format.Bold($"{song.Length.Minutes}:{song.Length.Seconds}"))
							.AppendLine();
						break;
					case 1:
						output.AppendFormat("Next : {0} {1} Length : {2}",
											Format.Bold(song.Name),
											SystemUtilities.GetSeparator,
											Format.Bold(song.Length.ToString()));
						break;
					default:
						output.AppendFormat("{0} : {1} {2} Length : {3}",
											Format.Code($"[{i}]"),
											Format.Bold(song.Name),
											SystemUtilities.GetSeparator,
											Format.Bold(song.Length.ToString()));
						break;
				}

				output.AppendLine();
				i++;
			}

			if (output.Length > 1950) 
				return (Context.Channel as ITextChannel).SendStringAsFileAsync("Queue.txt", output.ToString(), $"Queued Music Requests ({player.Requests.Count})");
			return ReplyAsync(output.ToString());
		}

		[Command("show")]
		public async Task ShowAsync()
		{
			var data = Context.Database.ServerMusics.GetData(Context.Guild.Id);
			var songCount = 0;

			if (data == null) {
				await ReplyAsync("There are no settings available for Music.");
				return;
			} else if (data.PlaylistId != ObjectId.Empty) {
				var playlist = await _musicService.Playlist.GetPlaylistAsync(data.PlaylistId);
				songCount = playlist.Songs.Count;
			}

			var vc = data.AutoVoiceChannel != 0 ? await Context.Guild.GetVoiceChannelAsync(data.AutoVoiceChannel) : null;
			var tc = data.AutoTextChannel != 0 ? await Context.Guild.GetTextChannelAsync(data.AutoTextChannel) : null;
			var npTc = data.NowPlayingChannel != 0 ? await Context.Guild.GetTextChannelAsync(data.NowPlayingChannel) : null;
			var autoJoinOutput = string.Format("{0} {1}", vc != null ? vc.Name : "Disabled", tc != null ? $"(#{tc.Name})" : "");
			var autoDownloadOutput = data.AutoDownload ? "Enabled" : "Disabled";
			var autoSkipOutput = data.AutoSkip ? "Enabled" : "Disabled";
			var silentPlayingOutput = data.SilentNowPlaying ? "Enabled" : "Disabled";
			var silentInstallOutput = data.SilentSongProcessing ? "Enabled" : "Disabled";
			var npTcName = npTc != null ? $"#{npTc.Name}" : "None";
			var voteskipOutput = data.VoteSkipEnabled ? $"Enabled @ {data.VoteSkipLimit}% Users" : "Disabled";
			var output = new StringBuilder();
			var roleLock = new StringBuilder();

			if (data.AllowedRoles.Count > 0)
				foreach (var allowedRole in data.AllowedRoles) {
					var role = Context.Guild.GetRole(allowedRole.RoleId);
					roleLock.AppendFormat("| {0} |", role.Name);
				} else roleLock.AppendFormat("None.");

			output.AppendLine("Music Settings")
				.AppendLine()
				.AppendFormat("Number Of Songs : {0}", songCount).AppendLine()
				.AppendLine()
				.AppendFormat("      Auto-Join : {0}", autoJoinOutput).AppendLine()
				.AppendFormat("  Auto-Download : {0}", autoDownloadOutput).AppendLine()
				.AppendFormat("      Auto-Skip : {0}", autoSkipOutput).AppendLine()
				.AppendLine()
				.AppendFormat(" Silent Running : {0}", silentPlayingOutput).AppendLine()
				.AppendFormat(" Silent Install : {0}", silentInstallOutput).AppendLine()
				.AppendLine()
				.AppendFormat("NP Dedi Channel : {0}", npTcName).AppendLine()
				.AppendFormat("      Vote-Skip : {0}", voteskipOutput).AppendLine()
				.AppendFormat("    Role-Locked : {0}", roleLock.ToString());

			await ReplyAsync(Format.Code(output.ToString()));
		}
	}
}