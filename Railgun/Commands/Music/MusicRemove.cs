using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Finite.Commands;
using MongoDB.Bson;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Music;
using TreeDiagram;

namespace Railgun.Commands.Music
{
	public partial class Music
	{
		[Alias("remove"), UserPerms(GuildPermission.ManageGuild)]
		public class MusicRemove : SystemBase
		{
			private readonly PlayerController _playerController;
			private readonly MusicService _musicService;

			public MusicRemove(PlayerController playerController, MusicService musicService)
			{
				_playerController = playerController;
				_musicService = musicService;
			}

			[Command]
			public async Task RemoveAsync([Remainder] string ids)
			{
				var data = Context.Database.ServerMusics.GetData(Context.Guild.Id);

				if (data == null || data.PlaylistId == ObjectId.Empty)
				{
					await ReplyAsync("Unknown Music Id Given!");
					return;
				}
				if (string.IsNullOrWhiteSpace(ids))
				{
					await ReplyAsync("Please specify a song to remove by using it's ID. An example of an ID is \"YOUTUBE#abcde123456\". To remove the currently playing song, please type \"current\" instead of the ID.");
					return;
				}

				var playlist = await _musicService.Playlist.GetPlaylistAsync(data.PlaylistId);
				var output = new StringBuilder();
				var playlistUpdated = false;

				foreach (var url in ids.Split(',', ' ')) {
					var cleanUrl = url.Trim('<','>');
					string id;
					var tempId = YoutubeExplode.Videos.VideoId.TryParse(cleanUrl);

					if (cleanUrl.Contains("#")) id = cleanUrl;
					else if (tempId != null) id = $"YOUTUBE#{tempId.Value}";
					else continue;

					var song = await _musicService.TryGetSongAsync(SongId.Parse(id));

					if (!song.Item1) {
						output.AppendFormat("{0} - Unknown Music Id Given!", id).AppendLine();
						continue;
					} else if (!playlist.Songs.Contains(song.Item2.Id)) {
						output.AppendFormat("{0} - Unknown Music Id Given!", id).AppendLine();
						continue;
					}

					playlist.Songs.Remove(song.Item2.Id);
					playlistUpdated = true;
					output.AppendFormat("{0} - Song Removed", id);
				}

				if (playlistUpdated) await _musicService.Playlist.UpdateAsync(playlist);

				await ReplyAsync(output.ToString());
			}

			[Command("current")]
			public async Task CurrentAsync()
			{
				var playerContainer = _playerController.GetPlayer(Context.Guild.Id);

				if (playerContainer == null) {
					await ReplyAsync("Cannot use this command if there's no active music stream at this time.");
					return;
				}

				var player = playerContainer.Player;
				var data = Context.Database.ServerMusics.GetData(Context.Guild.Id);

				if (data == null || data.PlaylistId == ObjectId.Empty || player == null) {
					await ReplyAsync("Can not remove current song because I am not in voice channel.");
					return;
				}

				var playlist = await _musicService.Playlist.GetPlaylistAsync(data.PlaylistId);
				playlist.Songs.Remove(player.CurrentSong.Id);

				await _musicService.Playlist.UpdateAsync(playlist);
				await ReplyAsync("Removed from playlist. Skipping to next song...");

				player.CancelMusic();
			}
		}
	}
}