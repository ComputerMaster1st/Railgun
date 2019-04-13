using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Configuration;
using Railgun.Core.Containers;
using Railgun.Core.Enums;
using Railgun.Core.Extensions;
using Railgun.Core.Music;
using Railgun.Core.Music.PlayerEventArgs;
using Railgun.Core.Utilities;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Core.Managers
{
    public class PlayerManager
	{
		private readonly IServiceProvider _services;
		private readonly MasterConfig _masterConfig;
		private readonly IDiscordClient _client;
		private readonly Log _log;
		private readonly CommandUtils _commandUtils;
		private readonly MusicService _musicService;

		public List<PlayerContainer> PlayerContainers = new List<PlayerContainer>();

		public PlayerManager(IServiceProvider services)
		{
			_services = services;

			_masterConfig = _services.GetService<MasterConfig>();
			_client = _services.GetService<IDiscordClient>();
			_log = _services.GetService<Log>();
			_commandUtils = _services.GetService<CommandUtils>();
			_musicService = _services.GetService<MusicService>();
		}

		public async Task CreatePlayerAsync(IGuildUser user, IVoiceChannel vc, ITextChannel tc, bool autoJoin = false, ISong preRequestedSong = null)
		{
			Playlist playlist;
			ServerMusic data;

			using (var scope = _services.CreateScope()) {
				var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
				data = db.ServerMusics.GetOrCreateData(tc.GuildId);

				playlist = await _commandUtils.GetPlaylistAsync(data);
			}

			if (playlist.Songs.Count < 1) {
				if (preRequestedSong != null && !playlist.Songs.Contains(preRequestedSong.Id))
					playlist.Songs.Add(preRequestedSong.Id);

				await tc.TrySendMessageAsync("As this server has no music yet, I've decided to gather 100 random songs from my repository. One momemt please...");

				var repository = (await _musicService.GetAllSongsAsync()).ToList();
				var rand = new Random();

				if (repository.Count < 100) foreach (var song in repository) playlist.Songs.Add(song.Id);
				else while (playlist.Songs.Count < 100) {
						var i = rand.Next(0, repository.Count());
						var song = repository.ElementAtOrDefault(i);

						if (song != null && !playlist.Songs.Contains(song.Id)) playlist.Songs.Add(song.Id);
					}

				await _musicService.Playlist.UpdateAsync(playlist);
			}

			var username = _commandUtils.GetUsernameOrMention(user);

			await tc.TrySendMessageAsync($"{(autoJoin ? "Music Auto-Join triggered by" : "Joining now")} {Format.Bold(username)}. Standby...");

			var player = new Player(_musicService, vc) {
				PlaylistAutoLoop = data.PlaylistAutoLoop
			};

			async void Connected(object s, ConnectedPlayerEventArgs a) => await ConnectedAsync(a);
			player.Connected += Connected;
			async void Playing(object s, CurrentSongPlayerEventArgs a) => await PlayingAsync(a);
			player.Playing += Playing;
			async void Timeout(object s, TimeoutPlayerEventArgs a) => await TimeoutAsync(a);
			player.Timeout += Timeout;
			async void Finished(object s, FinishedPlayerEventArgs a) => await FinishedAsync(a);
			player.Finished += Finished;

			if (preRequestedSong != null) {
				player.AddSongRequest(preRequestedSong);
				player.AutoSkipped = true;
			}

			var container = new PlayerContainer(tc, player);

			await CreateOrModifyMusicPlayerLogEntryAsync(container);
			await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, "Music", $"{(autoJoin ? "Auto-" : "")}Connecting..."));
			
			player.StartPlayer(playlist.Id);
			PlayerContainers.Add(container);
		}

		private async Task CreateOrModifyMusicPlayerLogEntryAsync(PlayerContainer container) {
			var song = container.Player.CurrentSong;
			var songId = "N/A";
			var songName = "N/A";
			var songLength = "N/A";
			var songStarted = "N/A";

			if (song != null) {
				songId = song.Id.ToString();
				songName = song.Metadata.Name;
				songLength = song.Metadata.Length.ToString();
				songStarted = container.Player.SongStartedAt.ToString(CultureInfo.CurrentCulture);
			}
			
			var output = new StringBuilder()
				.AppendFormat("Music Player {0} <{1} <{2}>>", Response.GetSeparator(), container.TextChannel.Guild.Name,
					container.TextChannel.GuildId).AppendLine()
				.AppendFormat("---- Created At      : {0}", container.Player.CreatedAt).AppendLine()
				.AppendFormat("---- Latency         : {0}ms", container.Player.Latency).AppendLine()
				.AppendFormat("---- Status          : {0}", container.Player.Status).AppendLine()
				.AppendFormat("---- Song Started At : {0}", songStarted).AppendLine()
				.AppendFormat("---- Song ID         : {0}", songId).AppendLine()
				.AppendFormat("---- Song Name       : {0}", songName).AppendLine()
				.AppendFormat("---- Song Length     : {0}", songLength).AppendLine();

			var formattedOutput = Format.Code(output.ToString());

			try
			{
				if (container.LogEntry != null)
				{
					await container.LogEntry.ModifyAsync((x) => x.Content = formattedOutput);
					return;
				}

				if (_masterConfig.DiscordConfig.BotLogChannels.MusicPlayerActive == 0) return;

				var masterGuild = await _client.GetGuildAsync(_masterConfig.DiscordConfig.MasterGuildId);
				if (masterGuild == null) return;
				var logTc = await masterGuild.GetTextChannelAsync(_masterConfig.DiscordConfig.BotLogChannels
					.MusicPlayerActive);
				if (logTc == null) return;

				container.LogEntry = await logTc.TrySendMessageAsync(formattedOutput);
			}
			catch
			{
				// Ignore
			}
		}

		public PlayerContainer GetPlayer(ulong playerId)
			=> PlayerContainers.FirstOrDefault(container => container.GuildId == playerId);

		public bool IsCreated(ulong playerId)
		{
			if (PlayerContainers.Any(container => container.GuildId == playerId)) return true;
			return false;
		}

		public void DisconnectPlayer(ulong playerId)
			=> PlayerContainers.First(container => container.GuildId == playerId).Player.CancelStream();

		private async Task StopPlayerAsync(ulong playerId, bool autoLeave = false)
		{
			var container = PlayerContainers.FirstOrDefault(cnt => cnt.GuildId == playerId);

			if (container == null) return;

			var player = container.Player;

			if (!autoLeave) player.CancelStream();

			while (player.PlayerTask.Status == TaskStatus.WaitingForActivation) await Task.Delay(500);

			player.PlayerTask.Dispose();
			PlayerContainers.Remove(PlayerContainers.First(cnt => cnt.GuildId == playerId));

			await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, "Music", $"Player ID '{playerId}' Destroyed"));
		}

		private async Task ConnectedAsync(ConnectedPlayerEventArgs args)
		{
			var container = PlayerContainers.First(x => x.GuildId == args.GuildId);
			await CreateOrModifyMusicPlayerLogEntryAsync(container);
		}

		private async Task PlayingAsync(CurrentSongPlayerEventArgs args)
		{
			try {
				ServerMusic data;
				ITextChannel tc;
				var container = PlayerContainers.First(x => x.GuildId == args.GuildId);

				using (var scope = _services.CreateScope()) {
					data = scope.ServiceProvider.GetService<TreeDiagramContext>().ServerMusics.GetData(args.GuildId);
				}

				if (data.NowPlayingChannel != 0)
					tc = await (await _client.GetGuildAsync(args.GuildId)).GetTextChannelAsync(data.NowPlayingChannel);
				else tc = container.TextChannel;

				if (!data.SilentNowPlaying) {
					var output = new StringBuilder()
						.AppendFormat("Now Playing: {0} {1} ID: {2}", Format.Bold(args.Song.Metadata.Name), Response.GetSeparator(), Format.Bold(args.Song.Id.ToString())).AppendLine()
						.AppendFormat("Time: {0} {1} Uploader: {2} {1} URL: {3}", Format.Bold(args.Song.Metadata.Length.ToString()), Response.GetSeparator(), Format.Bold(args.Song.Metadata.Uploader), Format.Bold($"<{args.Song.Metadata.Url}>"));

					await tc.TrySendMessageAsync(output.ToString());
				}

				await CreateOrModifyMusicPlayerLogEntryAsync(container);
			} catch {
				await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "Music", $"{args.GuildId} Missing TC!"));
				var container = PlayerContainers.FirstOrDefault(cnt => cnt.GuildId == args.GuildId);
				container?.Player.CancelStream();
			}
		}

		private async Task TimeoutAsync(TimeoutPlayerEventArgs args)
		{
			var tc = PlayerContainers.First(container => container.GuildId == args.GuildId).TextChannel;

			try {
				await tc.TrySendMessageAsync("Connection to Discord Voice has timed out! Please try again.");
			} catch {
				await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "Music", $"{args.GuildId} Missing TC!"));
			} finally {
				var output = new StringBuilder()
					.AppendFormat("<{0} ({1})> Action Timeout!", tc.Guild.Name, args.GuildId).AppendLine()
					.AppendFormat("---- Exception : {0}", args.Exception.ToString());

				await _log.LogToBotLogAsync(output.ToString(), BotLogType.MusicPlayerError);
			}
		}

		private async Task FinishedAsync(FinishedPlayerEventArgs args)
		{
			var container = PlayerContainers.FirstOrDefault(cnt => cnt.GuildId == args.GuildId);

			if (container == null) return;

			var tc = container.TextChannel;

			try {
				var output = new StringBuilder();

				if (args.Exception != null) {
					await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Error, "Music", $"{tc.GuildId} Exception!", args.Exception));

					var logOutput = new StringBuilder()
						.AppendFormat("<{0} ({1})> Music Player Error!", tc.Guild.Name, tc.GuildId).AppendLine()
						.AppendFormat("---- Error : {0}", args.Exception.ToString());

					await _log.LogToBotLogAsync(logOutput.ToString(), BotLogType.MusicPlayerError);

					output.AppendLine("An error has occured while playing! The stream has been automatically reset. You may start playing music again at any time.");
				}

				var autoOutput = args.AutoDisconnected ? "Auto-" : "";

				output.AppendFormat("{0}Left Voice Channel", autoOutput);

				await tc.TrySendMessageAsync(output.ToString());
			} catch {
				await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "Music", $"{args.GuildId} Missing TC!"));
				await _log.LogToBotLogAsync($"<{tc.Guild.Name} ({args.GuildId})> Crash-Disconnected", BotLogType.MusicPlayerError);
			} finally {
				await container.LogEntry.DeleteAsync();
				await StopPlayerAsync(args.GuildId, args.AutoDisconnected);
			}
		}
	}
}