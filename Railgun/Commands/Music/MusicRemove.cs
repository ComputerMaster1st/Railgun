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

				if (data == null || data.PlaylistId == ObjectId.Empty) {
					await ReplyAsync("Unknown Music Id Given!");

					return;
				}

				var playlist = await _musicService.Playlist.GetPlaylistAsync(data.PlaylistId);
				var output = new StringBuilder();
				var playlistUpdated = false;

				foreach (var id in ids.Split(',', ' ')) {
					if (!id.Contains('#')) continue;

					ISong song = null;

					if (!await _musicService.TryGetSongAsync(SongId.Parse(id), songOutput => song = songOutput)) {
						output.AppendFormat("{0} - Unknown Music Id Given!", id).AppendLine();
						continue;
					} else if (!playlist.Songs.Contains(song.Id)) {
						output.AppendFormat("{0} - Unknown Music Id Given!", id).AppendLine();
						continue;
					}

					playlist.Songs.Remove(song.Id);
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
				var song = player.GetFirstSongRequest();

				if (song == null) {
					await ReplyAsync("No song has been selected yet. Try this command again once a song starts playing.");

					return;
				}

				playlist.Songs.Remove(song.Id);

				await _musicService.Playlist.UpdateAsync(playlist);
				await ReplyAsync("Removed from playlist. Skipping to next song...");

				player.CancelMusic();
			}
		}
	}
}