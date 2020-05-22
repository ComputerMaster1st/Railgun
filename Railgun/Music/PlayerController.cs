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
using Railgun.Music.Scheduler;
using TreeDiagram;
using TreeDiagram.Models.Server;
using YoutubeExplode;

namespace Railgun.Music
{
    public class PlayerController
    {
        private readonly MasterConfig _config;
        private readonly IDiscordClient _client;
        private readonly BotLog _botLog;
        private readonly IServiceProvider _services;
        private readonly MusicService _musicService;
		private readonly MetaDataEnricher _enricher;
		private readonly YoutubeClient _ytClient;

        public List<PlayerContainer> PlayerContainers { get; } = new List<PlayerContainer>();

        public PlayerController(MasterConfig config, IDiscordClient client, BotLog botLog, MusicService musicService, MetaDataEnricher enricher, YoutubeClient ytClient, IServiceProvider services)
        {
            _config = config;
            _client = client;
            _botLog = botLog;
			_musicService = musicService;
			_enricher = enricher;
			_ytClient = ytClient;
            _services = services;
        }

        public async Task CreatePlayerAsync(IGuildUser user, IVoiceChannel vc, ITextChannel tc, bool autoJoin = false, SongRequest preRequestedSong = null)
		{
			Playlist playlist;
			ServerMusic data;
            string username;

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

				await tc.SendMessageAsync("As this server has no music yet, I've decided to gather some random songs from my repository. One moment please...");

                playlist.Songs.AddRange(_musicService.GetRandomSongs().Where(id => !playlist.Songs.Contains(id)).Select(id => id));

				await _musicService.Playlist.UpdateAsync(playlist);
			}

			await tc.SendMessageAsync($"{(autoJoin ? "Music Auto-Join triggered by" : "Joining now")} {Format.Bold(username)}. Standby...");

            if (PlayerContainers.Any(c => c.GuildId == tc.GuildId)) return;
            var container = new PlayerContainer(tc);
            PlayerContainers.Add(container);
            var player = new Player(_musicService, vc, new MusicScheduler(_musicService, playlist.Id, data.PlaylistAutoLoop, _ytClient, _enricher));

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
				.LoadEvent(new QueueFailEvent(this, _botLog))
                .LoadEvent(new FinishedEvent(this, _botLog))
			);

			if (preRequestedSong != null) {
				await player.MusicScheduler.AddSongRequestAsync(preRequestedSong);
				player.AutoSkipped = true;
			}

			await PlayerUtilities.CreateOrModifyMusicPlayerLogEntryAsync(_config, _client, container);

			SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Info, "Music", $"{(autoJoin ? "Auto-" : "")}Connecting..."));
			player.StartPlayer();
		}

		public PlayerContainer GetPlayer(ulong playerId)
			=> PlayerContainers.FirstOrDefault(container => container.GuildId == playerId);
		
		public bool DisconnectPlayer(ulong playerId) {
			var container = PlayerContainers.FirstOrDefault(c => c.GuildId == playerId);

            if (container is null) return false;
            if (container.Player is null)
            {
                PlayerContainers.Remove(container);
                return true;
            }

			container.Player.CancelStream();
            return true;
		}
		
		public async Task StopPlayerAsync(ulong playerId, bool autoLeave = false)
		{
			var container = PlayerContainers.FirstOrDefault(cnt => cnt.GuildId == playerId);

			if (container is null) return;

			var player = container.Player;

			if (!autoLeave) player.CancelStream();

			while (player.Status != PlayerStatus.Disconnected) await Task.Delay(500);
			PlayerContainers.Remove(container);

			SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Info, "Music", $"Player ID '{playerId}' Destroyed"));
		}
    }
}