using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core;
using Railgun.Core.Configuration;
using Railgun.Core.Containers;
using Railgun.Core.Enums;
using Railgun.Music.Events;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Music
{
    public class PlayerController
    {
        private readonly MasterConfig _config;
        private readonly IDiscordClient _client;
        private readonly BotLog _botLog;
        private readonly IServiceProvider _services;
        private readonly MusicService _musicService;

        public List<PlayerContainer> PlayerContainers { get; } = new List<PlayerContainer>();

        public PlayerController(MasterConfig config, IDiscordClient client, BotLog botLog, MusicService musicService, IServiceProvider services)
        {
            _config = config;
            _client = client;
            _botLog = botLog;
			_musicService = musicService;
            _services = services;
        }

        public async Task CreatePlayerAsync(IGuildUser user, IVoiceChannel vc, ITextChannel tc, bool autoJoin = false, ISong preRequestedSong = null)
		{
			Playlist playlist;
			ServerMusic data;
            string username;

			if (PlayerContainers.Any(c => c.GuildId == tc.GuildId)) return;

			var container = new PlayerContainer(tc);
			PlayerContainers.Add(container);

			using (var scope = _services.CreateScope())
            {
				var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
				data = db.ServerMusics.GetOrCreateData(tc.GuildId);
				playlist = await SystemUtilities.GetPlaylistAsync(_musicService, data);
                username = SystemUtilities.GetUsernameOrMention(db, user);
			}

			if (playlist.Songs.Count < 1)
            {
				if (preRequestedSong != null && !playlist.Songs.Contains(preRequestedSong.Id))
					playlist.Songs.Add(preRequestedSong.Id);

				await tc.SendMessageAsync("As this server has no music yet, I've decided to gather 100 random songs from my repository. One moment please...");

				var repository = await _musicService.EnumerateSongMetadataAsync();
				var rand = new Random();

				if (repository.Count() < 100) foreach (var song in repository) playlist.Songs.Add(song.Id);
				else while (playlist.Songs.Count < 100)
                    {
						var i = rand.Next(0, repository.Count());
						var song = repository.ElementAtOrDefault(i);
						if (song != null && !playlist.Songs.Contains(song.Id)) playlist.Songs.Add(song.Id);
					}

				await _musicService.Playlist.UpdateAsync(playlist);
			}

			await tc.SendMessageAsync($"{(autoJoin ? "Music Auto-Join triggered by" : "Joining now")} {Format.Bold(username)}. Standby...");

			var player = new Player(_musicService, vc) { PlaylistAutoLoop = data.PlaylistAutoLoop };

			try
			{
				await player.ConnectToVoiceAsync();
			}
			catch (Exception ex)
			{
				var failOutput = new StringBuilder()
					.AppendLine("Failed to connect to Discord Voice! If this problem persists, try changing voice/server regions.")
					.AppendLine()
					.AppendFormat("{0} {1}", Format.Bold("ERROR :"), ex.Message);

				await tc.SendMessageAsync(failOutput.ToString());
				PlayerContainers.Remove(container);
				return;
			}

			container.AddPlayer(player);
            container.AddEventLoader(new PlayerEventLoader(container)
                .LoadEvent(new ConnectedEvent(_config, _client))
                .LoadEvent(new PlayingEvent(_config, _client, _services))
                .LoadEvent(new TimeoutEvent(_botLog))
                .LoadEvent(new FinishedEvent(this, _botLog))
			);

			if (preRequestedSong != null) {
				player.AddSongRequest(preRequestedSong);
				player.AutoSkipped = true;
			}

			await PlayerUtilities.CreateOrModifyMusicPlayerLogEntryAsync(_config, _client, container);

			SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Info, "Music", $"{(autoJoin ? "Auto-" : "")}Connecting..."));
			player.StartPlayer(playlist.Id);
		}

		public PlayerContainer GetPlayer(ulong playerId)
			=> PlayerContainers.FirstOrDefault(container => container.GuildId == playerId);
		
		public void DisconnectPlayer(ulong playerId) {
			var container = PlayerContainers.FirstOrDefault(c => c.GuildId == playerId);
			if (!(container is null)) container.Player.CancelStream();
		}
		
		public async Task StopPlayerAsync(ulong playerId, bool autoLeave = false)
		{
			var container = PlayerContainers.FirstOrDefault(cnt => cnt.GuildId == playerId);

			if (container == null) return;

			var player = container.Player;

			if (!autoLeave) player.CancelStream();

			while (player.Status != PlayerStatus.Disconnected) await Task.Delay(500);
			PlayerContainers.Remove(PlayerContainers.First(cnt => cnt.GuildId == playerId));

			SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Info, "Music", $"Player ID '{playerId}' Destroyed"));
		}
    }
}