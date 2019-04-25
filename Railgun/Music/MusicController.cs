using System;
using System.Collections.Generic;
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

			foreach (var url in urls) 
            {
				var response = await tc.SendMessageAsync($"{Format.Bold("Processing :")} <{url}>...");
				var cleanUrl = url.Trim(' ', '<', '>');
				ISong song = null;

				if (!_musicService.Youtube.TryParseYoutubeUrl(url, out var videoId)) 
                {
					await response.ModifyAsync(properties => properties.Content = $"{Format.Bold("Invalid Url :")} <{cleanUrl}>");
					continue;
				}
				if (await _musicService.TryGetSongAsync(new SongId("YOUTUBE", videoId), result => song = result)) 
                {
					if (playlist.Songs.Contains(song.Id))
						await response.ModifyAsync(properties => properties.Content = $"{Format.Bold("Already Installed :")} ({song.Id.ToString()}) {song.Metadata.Name}");
					else 
                    {
						playlist.Songs.Add(song.Id);
						playlistModified = true;

						await response.ModifyAsync(properties => properties.Content = $"{Format.Bold("Installed :")} ({song.Id.ToString()}) {song.Metadata.Name}");
					}

					continue;
				}

				try 
                {
					song = await _musicService.Youtube.DownloadAsync(new Uri(url));
					playlist.Songs.Add(song.Id);
					playlistModified = true;

					await response.ModifyAsync(properties => properties.Content = $"{Format.Bold("Encoded & Installed :")} ({song.Id.ToString()}) {song.Metadata.Name}");
				} 
                catch (Exception ex) 
                {
					await response.ModifyAsync(properties => properties.Content = $"{Format.Bold("Failed To Install :")} (<{cleanUrl}>), {ex.Message}");
				}
			}

			if (playlistModified) await _musicService.Playlist.UpdateAsync(playlist);

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