using System.Threading.Tasks;
using AudioChord;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Music;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    public partial class Music
	{
		[Alias("auto"), UserPerms(GuildPermission.ManageGuild)]
		public partial class MusicAuto : SystemBase
		{
			private readonly PlayerController _playerController;
			private readonly MusicService _music;

			public MusicAuto(PlayerController playerController, MusicService music)
			{
				_playerController = playerController;
				_music = music;
			}

			[Command("play")]
			public async Task PlayAsync(string input)
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Music;
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
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Music;

				data.AutoPlaySong = string.Empty;
				return ReplyAsync("Will no longer play specific song on auto-join.");
			}

			[Command("skip")]
			public Task SkipAsync()
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Music;

				data.AutoSkip = !data.AutoSkip;
				return ReplyAsync($"Music Auto-Skip is now {Format.Bold(data.AutoSkip ? "Enabled" : "Disabled")}.");
			}
		}
	}
}