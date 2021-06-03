using AudioChord;
using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;
using YoutubeExplode.Videos;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicAuto
        {
            [Alias("play")]
            public class MusicAutoPlay : SystemBase
            {
				private readonly MusicService _musicService;

				public MusicAutoPlay(MusicService musicService)
					=> _musicService = musicService;

				[Command]
				public async Task PlayAsync(string input)
				{
					var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
					var data = profile.Music;
					var playlist = await SystemUtilities.GetPlaylistAsync(_musicService, data);

					SongId songId;
					var videoId = VideoId.TryParse(input);

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

				[Command]
                public Task ExecuteAsync()
                {
                    var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                    var data = profile.Music;

                    data.AutoPlaySong = string.Empty;

                    return ReplyAsync("Will no longer play specific song on auto-join.");
                }
            }
        }
    }
}
