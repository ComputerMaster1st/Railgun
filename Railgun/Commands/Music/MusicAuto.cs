using System.Threading.Tasks;
using AudioChord;
using Discord;
using Finite.Commands;
using Google.Apis.YouTube.v3.Data;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Music;
using TreeDiagram;
using YoutubeExplode;

namespace Railgun.Commands.Music
{
	public partial class Music
	{
		[Alias("auto"), UserPerms(GuildPermission.ManageGuild)]
		public class MusicAuto : SystemBase
		{
			private readonly PlayerController _playerController;
			private readonly MusicService _music;

			public MusicAuto(PlayerController playerController, MusicService music)
			{
				_playerController = playerController;
				_music = music;
			}

			[Command("join")]
			public Task JoinAsync()
			{
				var data = Context.Database.ServerMusics.GetOrCreateData(Context.Guild.Id);
				var vc = (Context.Author as IGuildUser).VoiceChannel;

				if (vc == null && data.AutoVoiceChannel == 0)
					return ReplyAsync("Music Auto-Join is currently disabled. Please join a voice channel and run this command again to enable it.");
				if (vc == null && data.AutoVoiceChannel != 0) {
					data.AutoVoiceChannel = 0;
					data.AutoTextChannel = 0;
					return ReplyAsync("Music Auto-Join has been disabled.");
				}

				data.AutoVoiceChannel = vc.Id;
				data.AutoTextChannel = Context.Channel.Id;

				return ReplyAsync($"{(data.AutoVoiceChannel == 0 ? "Music Auto-Join is now enabled!" : "")} Will automatically join {Format.Bold(vc.Name)} and use {Format.Bold("#" + Context.Channel.Name)} to post status messages.");
			}

			[Command("play")]
			public async Task PlayAsync(string input)
			{
				var data = Context.Database.ServerMusics.GetData(Context.Guild.Id);
				if (data == null)
				{
					await ReplyAsync("Music has not been configured yet! Please add/configure music to generate config.");
					return;
				}

				var playlist = await SystemUtilities.GetPlaylistAsync(_music, data);

				SongId songId;
				var videoId = YoutubeExplode.Videos.VideoId.TryParse(input);

				if (input.Contains("http") && videoId != null)
					songId = SongId.Parse("YOUTUBE#" + videoId.Value);
				else
					songId = SongId.Parse(input);

				if (!playlist.Songs.Contains(songId))
				{
					await ReplyAsync("Unable to find song in playlist! Please add it to playlist.");
					return;
				}
				if (data.AutoPlaySong == songId.ToString())
				{
					await ReplyAsync("This song is already set to play on auto-join.");
					return;
				}

				data.AutoPlaySong = songId.ToString();
				await ReplyAsync("Will now play specified song on auto-join!");
			}

			[Command("play")]
			public Task PlayAsync()
			{
				var data = Context.Database.ServerMusics.GetData(Context.Guild.Id);
				if (data == null) return ReplyAsync("Music has not been configured yet! Please add/configure music to generate config.");

				data.AutoPlaySong = string.Empty;
				return ReplyAsync("Will no longer play specific song on auto-join.");
			}

			[Command("skip")]
			public Task SkipAsync()
			{
				var data = Context.Database.ServerMusics.GetOrCreateData(Context.Guild.Id);
				data.AutoSkip = !data.AutoSkip;
				return ReplyAsync($"Music Auto-Skip is now {Format.Bold(data.AutoSkip ? "Enabled" : "Disabled")}.");
			}

			[Command("download")]
			public Task DownloadAsync()
			{
				var data = Context.Database.ServerMusics.GetOrCreateData(Context.Guild.Id);
				data.AutoDownload = !data.AutoDownload;
				return ReplyAsync($"Music Auto-Download is now {Format.Bold(data.AutoDownload ? "Enabled" : "Disabled")}.");
			}

			[Command("loop")]
			public Task AutoLoopAsync()
			{
				var data = Context.Database.ServerMusics.GetOrCreateData(Context.Guild.Id);
				data.PlaylistAutoLoop = !data.PlaylistAutoLoop;

				var container = _playerController.GetPlayer(Context.Guild.Id);
				if (container != null) container.Player.MusicScheduler.PlaylistAutoLoop = data.PlaylistAutoLoop;

				return ReplyAsync($"Music Playlist Auto-Loop is now {Format.Bold(data.PlaylistAutoLoop ? "Enabled" : "Disabled")}.");
			}
		}
	}
}