using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Apis.Youtube;
using Railgun.Core.Attributes;
using Railgun.Core.Configuration;
using Railgun.Core.Containers;
using Railgun.Core.Enums;
using TreeDiagram;
using TreeDiagram.Models.Server;
using Railgun.Music;
using YoutubeExplode;
using YoutubeExplode.Exceptions;

namespace Railgun.Commands.Music
{
    public partial class Music
	{
		[Alias("play")]
		public class MusicPlay : SystemBase
		{
			private readonly MasterConfig _config;
			private readonly BotLog _botLog;
			private readonly PlayerController _playerController;
			private readonly MusicService _musicService;
			private readonly MusicServiceConfiguration _musicConfig;
			private readonly MetaDataEnricher _enricher;
			private readonly YoutubeClient _ytClient;
			private bool _playOneTimeOnly;

			public MusicPlay(MasterConfig config, BotLog botLog, PlayerController playerController, MusicService musicService, MusicServiceConfiguration musicConfig, MetaDataEnricher enricher, YoutubeClient ytClient)
			{
				_config = config;
				_botLog = botLog;
				_playerController = playerController;
				_musicService = musicService;
				_musicConfig = musicConfig;
				_enricher = enricher;
				_ytClient = ytClient;
			}

			private async Task QueueSongAsync(PlayerContainer playerContainer, Playlist playlist, SongRequest song, ServerMusic data, IUserMessage response)
			{
				var nowInstalled = false;

				if (data.WhitelistMode && !playlist.Songs.Contains(song.Id))
                {
                    await response.Channel.SendMessageAsync($"{Format.Bold(song.Name)} from {Format.Bold(song.Uploader)} needs to be approved before it can play.");
					await response.DeleteAsync();
					return;
                }

				if (!playlist.Songs.Contains(song.Id) && !_playOneTimeOnly) {
					playlist.Songs.Add(song.Id);
					await _musicService.Playlist.UpdateAsync(playlist);
					nowInstalled = true;
				}

				var output = new StringBuilder()
					.AppendFormat("{0} {1} {2} Url: {3} {2} ID: {4} {2} Requested by {5}. {6}",
						nowInstalled ? "Queued" : _playOneTimeOnly ? "Queued (One-Time Only)" : "",
						Format.Bold(song.Name),
						SystemUtilities.GetSeparator,
						song.Id.ProcessorId == "YOUTUBE" ? Format.Bold($"https://youtu.be/{song.Id.SourceId}") : Format.Bold("Unknown"),
						song.Id.ToString(),
						Format.Bold(SystemUtilities.GetUsernameOrMention(Context.Database, Context.Author as IGuildUser)),
						playerContainer == null ? "Now starting music player..." : "")
					.AppendLine();

				var user = (IGuildUser)Context.Author;
				var vc = user.VoiceChannel;

				if (playerContainer == null) {
					await response.ModifyAsync(x => x.Content = output.ToString());
					await _playerController.CreatePlayerAsync(user, vc, (ITextChannel)Context.Channel, preRequestedSong: song);
					return;
				}

				var player = playerContainer.Player;

				if (player.VoiceChannel.Id != vc.Id) {
					await response.ModifyAsync(x => x.Content = "Please be in the same voice channel as me when requesting a song to play.");
					return;
				}

				await player.MusicScheduler.AddSongRequestAsync(song);

				if (data.AutoSkip && !player.AutoSkipped) {
					output.AppendLine("Auto-Skipping current song as requested.");
					player.SkipMusic();
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

				var playerContainer = _playerController.GetPlayer(Context.Guild.Id);
				var data = Context.Database.ServerMusics.GetOrCreateData(Context.Guild.Id);
				var playlist = await SystemUtilities.GetPlaylistAsync(_musicService, data);

				await Context.Database.SaveChangesAsync();

				var response = await ReplyAsync("Standby...");

				if (Context.Message.Attachments.Count > 0)
					await UploadAsync(playerContainer, playlist, data, response);
				else if (input.StartsWith("YOUTUBE#") || input.StartsWith("DISCORD#"))
					await AddByIdAsync(input, playerContainer, playlist, data, response);
				else if (input.StartsWith("http://") || input.StartsWith("https://"))
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
					_enricher.AddMapping($"{Context.Author.Username}#{Context.Author.DiscriminatorValue}", SongId.Parse($"DISCORD#{attachment.Id}"), attachment.Filename);
					var song = await _musicService.DownloadSongAsync(attachment.ProxyUrl);

					await QueueSongAsync(playerContainer, playlist, new SongRequest(song), data, response);
				} catch (Exception ex) {
					await response.ModifyAsync(x => x.Content = $"An error has occured! {Format.Bold("ERROR : ") + ex.Message}");

					var output = new StringBuilder()
						.AppendFormat("<{0} ({1})> Upload From Discord Failure!", Context.Guild.Name, Context.Guild.Id).AppendLine()
						.AppendLine(ex.ToString());

					await _botLog.SendBotLogAsync(BotLogType.MusicManager, output.ToString());
				}
			}

			private async Task AddByIdAsync(string input, PlayerContainer playerContainer, Playlist playlist, ServerMusic data, IUserMessage response)
			{
				var song = await _musicService.GetSongAsync(SongId.Parse(input));

				if (song != null) {
					await QueueSongAsync(playerContainer, playlist, new SongRequest(song), data, response);
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
				if (!input.Contains("youtu")) {
					await response.ModifyAsync(x => x.Content = "Only YouTube links can be processed.");
					return;
				}

				var videoId = YoutubeExplode.Videos.VideoId.TryParse(input);

				if (videoId == null) {
					await response.ModifyAsync(x => x.Content = "Invalid Youtube Video Link");
					return;
				}

				var song = await _musicService.GetSongAsync(new SongId("YOUTUBE", videoId));

				if (song != null)
				{
					if (playlist.Songs.Contains(song.Metadata.Id))
					{
						await QueueSongAsync(playerContainer, playlist, new SongRequest(song), data, response);
						return;
					}
					if (!data.AutoDownload)
					{
						await response.ModifyAsync(x => x.Content = "Unable to queue song! Auto-Download is disabled!");
						return;
					}

					await QueueSongAsync(playerContainer, playlist, new SongRequest(song), data, response);
					return;
				}

				try
				{
					var video = await _ytClient.Videos.GetAsync(videoId.Value);

					if (video.Duration > _musicConfig.ExtractorConfiguration.MaxSongDuration)
						throw new ArgumentOutOfRangeException($"Requested music is longer than {Format.Bold(_musicConfig.ExtractorConfiguration.MaxSongDuration.ToString(@"hh\:mm\:ss"))}");

					await QueueSongAsync(playerContainer, playlist, new SongRequest(new SongId("YOUTUBE", video.Id), video.Title, video.Duration, video.Author), data, response);
                } catch (RequestLimitExceededException ex) {
					await response.ModifyAsync(x => x.Content = $"An error has occured! {Format.Bold("ERROR : ") + "Youtube Rate-Limited (Error Code: 429)! Please try again later."}");

					var output = new StringBuilder()
						.AppendFormat("<{0} ({1})> Download From Youtube Failure!", Context.Guild.Name, Context.Guild.Id).AppendLine().AppendLine(ex.Message);
					await _botLog.SendBotLogAsync(BotLogType.MusicManager, output.ToString());
				} catch (Exception ex) {
					await response.ModifyAsync(x => x.Content = $"An error has occured! {Format.Bold("ERROR : ") + ex.Message}");

					var output = new StringBuilder()
						.AppendFormat("<{0} ({1})> Download From Youtube Failure!", Context.Guild.Name, Context.Guild.Id).AppendLine().AppendLine(ex.Message);
					await _botLog.SendBotLogAsync(BotLogType.MusicManager, output.ToString());
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