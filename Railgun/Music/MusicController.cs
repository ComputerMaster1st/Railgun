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
				var data = db.ServerMusics.GetOrCreateData(tc.GuildId);
				playlist = await SystemUtilities.GetPlaylistAsync(_musicService, data);

				await db.SaveChangesAsync();
			}

			var playlistModified = false;
			var invalidUrls = new List<string>();
			var installed = 0;
			var imported = 0;
			var encoded = 0;
			var failed = 0;

			await tc.SendMessageAsync($"Processing {Format.Bold(urls.Count().ToString())} song(s)... {(urls.Count() < 10 ? "" : "This may take some time depending on how many require downloading.")}");

			foreach (var url in urls) {
				var cleanUrl = url.Trim(' ', '<', '>');
				ISong song = null;

				if (!_musicService.Youtube.TryParseYoutubeUrl(url, out var videoId)) {
					invalidUrls.Add(url);
					continue;
				} 
				if (await _musicService.TryGetSongAsync(new SongId("YOUTUBE", videoId), result => song = result)) {
					if (playlist.Songs.Contains(song.Id)) installed++;
					else {
						playlist.Songs.Add(song.Id);
						playlistModified = true;
						imported++;
					}

					continue;
				}

				var response = await tc.SendMessageAsync($"{Format.Bold("Processing :")} <{url}>...");

				try {
					song = await _musicService.Youtube.DownloadAsync(new Uri(url));
					playlist.Songs.Add(song.Id);
					playlistModified = true;
					encoded++;

					await response.ModifyAsync(properties => properties.Content = $"{Format.Bold("Encoded & Installed :")} ({song.Id.ToString()}) {song.Metadata.Name}");
				} 
                catch (Exception ex) {
					failed++;
					await response.ModifyAsync(properties => properties.Content = $"{Format.Bold("Failed To Install :")} (<{cleanUrl}>), {ex.Message}");
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
				.AppendFormat("{0} - Invalid Urls", Format.Code($"[{invalidUrls.Count}]")).AppendLine();

			await Task.Delay(1000);
			await tc.SendMessageAsync("Done!");
		}

		public async Task ProcessYoutubePlaylistAsync(string url, Playlist playlist, ResolvingPlaylist resolvingPlaylist, ITextChannel tc)
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

			var newlyEncoded = (resolvingPlaylist.Songs.Count - resolvingPlaylist.ExistingSongs) - failed;

			var output = new StringBuilder()
				.AppendLine("Finished Processing YouTube Playlist! Results...")
				.AppendFormat(
				"Already Installed : {0} {1} Imported From Repository : {2} {1} Newly Encoded : {3} {1} Failed : {4}",
					Format.Bold(alreadyInstalled.ToString()),
					SystemUtilities.GetSeparator,
					Format.Bold(resolvingPlaylist.ExistingSongs.ToString()),
					Format.Bold(newlyEncoded.ToString()),
					Format.Bold(failed.ToString())
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
				.AppendFormat("---- Failed To Install  : {0}", failed).AppendLine();

			await tc.SendMessageAsync(output.ToString());
			await _botLog.SendBotLogAsync(BotLogType.AudioChord, logOutput.ToString());
		}

		public async Task YoutubePlaylistStatusUpdatedAsync(ITextChannel tc, SongProcessStatus status, ServerMusic data)
		{
			switch (status.Status) 
            {
				case SongStatus.Errored: 
                    {
						var output = (SongProcessError)status;
						var url = "https://youtu.be/" + output.Id.SourceId;
						
						try 
                        {
							await tc.SendMessageAsync($"{Format.Bold("Failed To Install :")} (<{url}>), {output.Exceptions.Message}");
						} 
                        catch (ArgumentException ex) 
                        {
							SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, "Music Manager", "Missing Playlist", ex));
						} 
                        catch (Exception ex) 
                        {
							SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, "Music Manager", "Missing TC", ex));
						}
					}
					break;
				case SongStatus.Processed: 
                    {
						var output = (SongProcessResult)status;
						var song = await output.Result;

						try 
                        {
							var playlist = await SystemUtilities.GetPlaylistAsync(_musicService, data);

							playlist.Songs.Add(song.Id);

							await _musicService.Playlist.UpdateAsync(playlist);
							await tc.SendMessageAsync($"{Format.Bold("Encoded & Installed :")} ({song.Id}) {song.Metadata.Name}");
						} 
                        catch (ArgumentException ex) 
                        {
							SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, "Music Manager", "Missing Playlist", ex));
						} 
                        catch (Exception ex) 
                        {
							SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, "Music Manager", "Missing TC", ex));
						}
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
    }
}