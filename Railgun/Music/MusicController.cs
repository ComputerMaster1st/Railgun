using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core;
using Railgun.Core.Enums;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Music
{
    public class MusicController
    {
        private readonly BotLog _botLog;
        private readonly MusicService _musicService;
        private readonly IServiceProvider _services;

        private const string YoutubeBaseUrl = "https://youtu.be/";

        public MusicController(BotLog botLog, MusicService musicService, IServiceProvider services)
		{
            _botLog = botLog;
            _musicService = musicService;
            _services = services;
		}

		public async Task AddYoutubeSongsAsync(IEnumerable<string> urls, ITextChannel tc)
		{
			Playlist playlist;

			using (var scope = _services.CreateScope()) 
            {
				var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
				playlist = await SystemUtilities.GetPlaylistAsync(_musicService, db.ServerMusics.GetOrCreateData(tc.GuildId));

				await db.SaveChangesAsync();
			}

			var playlistModified = false;
			var invalidUrls = 0;
			var installed = 0;
			var imported = 0;
			var encoded = 0;
			var failed = 0;
			var initialOutput = $"Processing {Format.Bold(urls.Count().ToString())} song(s)...";
			var initialResponse = await tc.SendMessageAsync(initialOutput);
            var videoIds = new List<string>();

            foreach (var url in urls) {
                var cleanUrl = url.Trim(' ', '<', '>');

                if (!_musicService.Youtube.TryParseYoutubeUrl(url, out var videoId)) {
                    invalidUrls++;
                    await tc.SendMessageAsync($"{Format.Bold("Invalid Url :")} {Format.EscapeUrl(url)}");
                    continue;
                }

                videoIds.Add(videoId);
            }

            foreach (var videoId in videoIds)
            { 
				var song = await _musicService.TryGetSongAsync(new SongId("YOUTUBE", videoId));
				if (song.Item1) {
					if (playlist.Songs.Contains(song.Item2.Id)) installed++;
					else {
						playlist.Songs.Add(song.Item2.Id);
						playlistModified = true;
						imported++;
					}

					continue;
				}

				await initialResponse.ModifyAsync(properties => properties.Content = $"{initialResponse} This may take some time depending on how many require downloading.");
				var response = await tc.SendMessageAsync($"{Format.Bold("Processing :")} {Format.EscapeUrl(YoutubeBaseUrl + videoId)}...");

				try {
					var downloadedSong = await _musicService.Youtube.DownloadAsync(new Uri(YoutubeBaseUrl + videoId));
					playlist.Songs.Add(downloadedSong.Id);
					playlistModified = true;
					encoded++;

					await response.ModifyAsync(properties => properties.Content = $"{Format.Bold("Encoded & Installed :")} ({downloadedSong.Id.ToString()}) {downloadedSong.Metadata.Name}");
				} 
                catch (Exception ex) {
					failed++;
					await response.ModifyAsync(properties => properties.Content = $"{Format.Bold("Failed To Install :")} ({Format.EscapeUrl(YoutubeBaseUrl + videoId)}), {ex.Message}");
				}
			}

			if (playlistModified) await _musicService.Playlist.UpdateAsync(playlist);

			var output = new StringBuilder()
				.AppendLine(Format.Bold(Format.Underline("Processing Completed!")))
				.AppendLine()
				.AppendFormat("{0} - Already Installed", Format.Code($"[{installed}]")).AppendLine()
				.AppendFormat("{0} - Imported From Repository", Format.Code($"[{imported}]")).AppendLine()
				.AppendFormat("{0} - Newly Encoded & Installed", Format.Code($"[{encoded}]")).AppendLine()
				.AppendFormat("{0} - Failed To Install", Format.Code($"[{failed}]")).AppendLine()
                .AppendFormat("{0} - Invalid Urls", Format.Code($"[{invalidUrls}]")).AppendLine();

            await Task.Delay(1000);
			await tc.SendMessageAsync("Done!");
		}

        public async Task ProcessYoutubePlaylistAsync(string url, Playlist playlist, ResolvingPlaylist resolvingPlaylist, ITextChannel tc, PlaylistResult result)
		{
			var alreadyInstalled = 0;
			var failed = 0;
			var startedAt = DateTime.Now;

			foreach (var songTask in resolvingPlaylist.Songs) 
            {
				try 
                {
					var song = songTask.Result;

					if (playlist.Songs.Contains(song.Id)) 
                    {
						alreadyInstalled++;
						continue;
					}

					playlist.Songs.Add(song.Id);

					await _musicService.Playlist.UpdateAsync(playlist);
				} 
                catch { failed++; }
			}

			var newlyEncoded = resolvingPlaylist.Songs.Count - resolvingPlaylist.ExistingSongs - failed;
            var errors = result.GetErrors();

			var output = new StringBuilder()
				.AppendLine("Finished Processing YouTube Playlist! Results...")
				.AppendFormat(
				"Already Installed : {0} {1} Imported From Repository : {2} {1} Newly Encoded : {3} {1} Failed : {4}",
					Format.Bold(alreadyInstalled.ToString()),
					SystemUtilities.GetSeparator,
					Format.Bold(resolvingPlaylist.ExistingSongs.ToString()),
					Format.Bold(newlyEncoded.ToString()),
					Format.Bold(errors.Failed.ToString())
				);

            var logOutput = new StringBuilder()
				.AppendFormat("<{0} <{1}>> YouTube Playlist Processed!", tc.Guild.Name, tc.GuildId).AppendLine()
				.AppendFormat("---- Url                : {0}", url).AppendLine()
				.AppendFormat("---- Started            : {0}", startedAt).AppendLine()
				.AppendFormat("---- Finished           : {0}", DateTime.Now).AppendLine()
				.AppendLine()
				.AppendFormat("---- Already Installed  : {0}", alreadyInstalled).AppendLine()
				.AppendFormat("---- Imported From Repo : {0}", resolvingPlaylist.ExistingSongs).AppendLine()
				.AppendFormat("---- Encoded/Installed  : {0}", newlyEncoded).AppendLine()
				.AppendFormat("---- Failed To Install  : {0}", errors.Failed).AppendLine();

            if (errors.Failed > 0)
            {
                output.AppendLine().AppendLine().AppendLine(errors.Message);
                logOutput.AppendLine().AppendLine(errors.Message);
            }

            await _botLog.SendBotLogAsync(BotLogType.AudioChord, logOutput.ToString());
            await tc.SendMessageAsync(output.ToString());
        }
    }
}