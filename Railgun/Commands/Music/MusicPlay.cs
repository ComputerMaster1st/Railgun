using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Finite.Commands;
using Railgun.Core.Api.Youtube;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using Railgun.Core.Configuration;
using Railgun.Core.Containers;
using Railgun.Core.Logging;
using Railgun.Core.Managers;
using Railgun.Core.Utilities;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Commands.Music
{
	public partial class Music
	{
		[Alias("play")]
		public class MusicPlay : SystemBase
		{
			private readonly MasterConfig _config;
			private readonly Log _log;
			private readonly CommandUtils _commandUtils;
			private readonly PlayerManager _playerManager;
			private readonly MusicService _musicService;
			private bool _playOneTimeOnly;

			public MusicPlay(MasterConfig config, Log log, CommandUtils commandUtils, PlayerManager playerManager, MusicService musicService)
			{
				_config = config;
				_log = log;
				_commandUtils = commandUtils;
				_playerManager = playerManager;
				_musicService = musicService;
			}

			private async Task QueueSongAsync(PlayerContainer playerContainer, Playlist playlist, ISong song, ServerMusic data, IUserMessage response)
			{
				var nowInstalled = false;

				if (!playlist.Songs.Contains(song.Id) && !_playOneTimeOnly) {
					playlist.Songs.Add(song.Id);

					await _musicService.Playlist.UpdateAsync(playlist);

					nowInstalled = true;
				}

				var output = new StringBuilder()
					.AppendFormat("{0} Queued {1} as requested by {2}. {3}",
						nowInstalled ? "Installed &" : _playOneTimeOnly ? "One-Time Only &" : "",
						Format.Bold(song.Metadata.Name),
						Format.Bold(_commandUtils.GetUsernameOrMention((IGuildUser)Context.Author)),
						playerContainer == null ? "Now starting music player..." : "").AppendLine();

				var user = (IGuildUser)Context.Author;
				var vc = user.VoiceChannel;

				if (playerContainer == null) {
					await response.ModifyAsync(x => x.Content = output.ToString());
					await _playerManager.CreatePlayerAsync(user, vc, (ITextChannel)Context.Channel, preRequestedSong: song);

					return;
				}

				var player = playerContainer.Player;

				if (player.VoiceChannel.Id != vc.Id) {
					await response.ModifyAsync(x => x.Content = "Please be in the same voice channel as me when requesting a song to play.");

					return;
				}

				player.AddSongRequest(song);

				if (data.AutoSkip && !player.AutoSkipped) {
					output.AppendLine("Auto-Skipping current song as requested.");
					player.CancelMusic();
				}

				player.AutoSkipped = true;

				await response.ModifyAsync(x => x.Content = output.ToString());
			}

			[Command]
			public async Task PlayAsync([Remainder] string input = null)
			{
				if (string.IsNullOrWhiteSpace(input) && Context.Message.Attachments.Count < 1) {
					await ReplyAsync("Please specify either a YouTube Link, Music Id, Search Query or upload an audio file.");

					return;
				} else if (((IGuildUser)Context.Author).VoiceChannel == null) {
					await ReplyAsync("You're not in a voice channel! Please join one.");

					return;
				}

				var playerContainer = _playerManager.GetPlayer(Context.Guild.Id);
				var data = Context.Database.ServerMusics.GetOrCreateData(Context.Guild.Id);
				var playlist = await _commandUtils.GetPlaylistAsync(data);

				await Context.Database.SaveChangesAsync();

				var response = await ReplyAsync("Standby...");

				if (Context.Message.Attachments.Count > 0)
					await UploadAsync(playerContainer, playlist, data, response);
				else if (input.Contains("YOUTUBE#") || input.Contains("DISCORD#"))
					await AddByIdAsync(input, playerContainer, playlist, data, response);
				else if (input.Contains("http://") || input.Contains("https://"))
					await AddByUrlAsync(input.Trim('<', '>'), playerContainer, playlist, data, response);
				else await SearchAsync(input, playerContainer, playlist, data, response);
			}

			[Command("onetime")]
			public Task PlayOneTimeAsync([Remainder] string input = null)
			{
				_playOneTimeOnly = true;

				return PlayAsync(input);
			}

			private async Task UploadAsync(PlayerContainer playerContainer, Playlist playlist, ServerMusic data, IUserMessage response)
			{
				try {
					var attachment = Context.Message.Attachments.FirstOrDefault();
					var song = await _musicService.Discord.DownloadAsync(attachment.ProxyUrl, $"{Context.Message.Author.Username}#{Context.Message.Author.DiscriminatorValue}", attachment.Id);

					await QueueSongAsync(playerContainer, playlist, song, data, response);
				} catch (Exception ex) {
					await response.ModifyAsync(x => x.Content = $"An error has occured! {Format.Bold("ERROR : ") + ex.Message}");

					var output = new StringBuilder()
						.AppendFormat("<{0} ({1})> Upload From Discord Failure!", Context.Guild.Name, Context.Guild.Id).AppendLine()
						.AppendLine(ex.ToString());

					await _log.LogToBotLogAsync(output.ToString(), BotLogType.MusicManager);
				}
			}

			private async Task AddByIdAsync(string input, PlayerContainer playerContainer, Playlist playlist, ServerMusic data, IUserMessage response)
			{
				ISong song = null;

				if (await _musicService.TryGetSongAsync(SongId.Parse(input), result => song = result)) {
					await QueueSongAsync(playerContainer, playlist, song, data, response);

					return;
				} else if (!input.Contains("YOUTUBE#")) {
					await response.ModifyAsync(x => x.Content = "Specified song does not exist.");

					return;
				}

				var ytUrl = $"https://youtu.be/{input.Split('#', 2).LastOrDefault()}";
				await AddByUrlAsync(ytUrl, playerContainer, playlist, data, response);
			}

			private async Task AddByUrlAsync(string input, PlayerContainer playerContainer, Playlist playlist, ServerMusic data, IUserMessage response)
			{
				var videoId = string.Empty;
				ISong song = null;

				if (!input.Contains("youtu")) {
					await response.ModifyAsync(x => x.Content = "Only YouTube links can be processed.");

					return;
				} else if (!_musicService.Youtube.TryParseYoutubeUrl(input, out videoId)) {
					await response.ModifyAsync(x => x.Content = "Invalid Youtube Video Link");

					return;
				} else if (await _musicService.TryGetSongAsync(new SongId("YOUTUBE", videoId), songOut => song = songOut)) {
					if (playlist.Songs.Contains(song.Id)) {
						await QueueSongAsync(playerContainer, playlist, song, data, response);

						return;
					} else if (!data.AutoDownload) {
						await response.ModifyAsync(x => x.Content = "Unable to queue song! Auto-Download is disabled!");

						return;
					}
				}

				try {
					song = await _musicService.Youtube.DownloadAsync(new Uri(input));

					await QueueSongAsync(playerContainer, playlist, song, data, response);
				} catch (Exception ex) {
					await response.ModifyAsync(x => x.Content = $"An error has occured! {Format.Bold("ERROR : ") + ex.Message}");

					var output = new StringBuilder()
						.AppendFormat("<{0} ({1})> Download From Youtube Failure!", Context.Guild.Name, Context.Guild.Id).AppendLine().AppendLine(ex.Message);

					await _log.LogToBotLogAsync(output.ToString(), BotLogType.MusicManager);
				}
			}

			private async Task SearchAsync(string input, PlayerContainer playerContainer, Playlist playlist, ServerMusic data, IUserMessage response)
			{
				if (string.IsNullOrEmpty(_config.GoogleApiToken)) {
					await response.ModifyAsync(x => x.Content = "Music Search is disabled due to no YouTube API V3 key being installed. Please contact the master admin.");

					return;
				} else if (string.IsNullOrWhiteSpace(input)) {
					await response.ModifyAsync(x => x.Content = "Search Query can not be empty.");

					return;
				}

				var search = new YoutubeSearch(_config);
				var video = await search.GetVideoAsync(input);

				if (video == null) {
					await response.ModifyAsync(x => x.Content = "Unable to find anything using that query.");

					return;
				}

				await AddByUrlAsync($"https://youtu.be/{video.Id}", playerContainer, playlist, data, response);
			}
		}
	}
}