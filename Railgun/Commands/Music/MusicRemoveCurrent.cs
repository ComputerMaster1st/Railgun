using AudioChord;
using Finite.Commands;
using MongoDB.Bson;
using Railgun.Core;
using Railgun.Music;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicRemove
        {
			[Alias("current")]
            public class MusicRemoveCurrent : SystemBase
            {
				private readonly MusicService _musicService;
				private readonly PlayerController _players;

				public MusicRemoveCurrent(MusicService musicService, PlayerController players)
                {
					_musicService = musicService;
					_players = players;
                }

				[Command]
				public async Task ExecueAsync()
				{
					var playerContainer = _players.GetPlayer(Context.Guild.Id);

					if (playerContainer == null)
					{
						await ReplyAsync("Cannot use this command if there's no active music stream at this time.");
						return;
					}

					var player = playerContainer.Player;
					var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
					var data = profile.Music;

					if (data.PlaylistId == ObjectId.Empty || player == null)
					{
						await ReplyAsync("Can not remove current song because I am not in voice channel.");
						return;
					}

					var playlist = await SystemUtilities.GetPlaylistAsync(_musicService, data);
					playlist.Songs.Remove(player.CurrentSong.Metadata.Id);

					await SystemUtilities.UpdatePlaylistAsync(_musicService, playlist);
					await ReplyAsync("Removed from playlist. Skipping to next song...");

					player.SkipMusic();
				}
			}
        }
    }
}
