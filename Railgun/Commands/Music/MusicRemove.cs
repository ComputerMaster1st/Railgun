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
		public partial class MusicRemove : SystemBase
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
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Music;

				if (data.PlaylistId == ObjectId.Empty)
				{
					await ReplyAsync("Unknown Music Id Given!");
					return;
				}
				if (string.IsNullOrWhiteSpace(ids))
				{
					await ReplyAsync("Please specify a song to remove by using it's ID. An example of an ID is \"YOUTUBE#abcde123456\". To remove the currently playing song, please type \"current\" instead of the ID.");
					return;
				}

				var playlist = await SystemUtilities.GetPlaylistAsync(_musicService, data);
				var output = new StringBuilder();
				var playlistUpdated = false;

				foreach (var url in ids.Split(',', ' ')) {
					var cleanUrl = url.Trim('<','>');
					string id;
					var tempId = YoutubeExplode.Videos.VideoId.TryParse(cleanUrl);

					if (cleanUrl.Contains("#")) id = cleanUrl;
					else if (tempId != null) id = $"YOUTUBE#{tempId.Value}";
					else continue;

					var song = await _musicService.GetSongAsync(SongId.Parse(id));

					if (song == null) {
						output.AppendFormat("{0} - Unknown Music Id Given!", id).AppendLine();
						continue;
					} else if (!playlist.Songs.Contains(song.Metadata.Id)) {
						output.AppendFormat("{0} - Unknown Music Id Given!", id).AppendLine();
						continue;
					}

					playlist.Songs.Remove(song.Metadata.Id);
					playlistUpdated = true;
					output.AppendFormat("{0} - Song Removed", id);
				}

				if (playlistUpdated) await SystemUtilities.UpdatePlaylistAsync(_musicService, playlist);

				await ReplyAsync(output.ToString());
			}
		}
	}
}