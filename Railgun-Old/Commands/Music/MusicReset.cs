using System.Threading.Tasks;
using AudioChord;
using Discord;
using Finite.Commands;
using MongoDB.Bson;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using Railgun.Core.Configuration;
using Railgun.Core.Managers;
using TreeDiagram;

namespace Railgun.Commands.Music
{
	public partial class Music
	{
		[Alias("reset"), UserPerms(GuildPermission.ManageGuild)]
		public class MusicReset : SystemBase
		{
			private readonly MasterConfig _config;
			private readonly PlayerManager _playerManager;
			private readonly MusicService _musicService;
			private bool _full;

			public MusicReset(MasterConfig config, PlayerManager playerManager, MusicService musicService)
			{
				_config = config;
				_playerManager = playerManager;
				_musicService = musicService;
			}

			[Command("stream")]
			public async Task StreamAsync()
			{
				if (!_playerManager.IsCreated(Context.Guild.Id)) {
					if (!_full) await ReplyAsync("I'm not streaming any music at this time.");

					return;
				}

				_playerManager.DisconnectPlayer(Context.Guild.Id);

				if (!_full) await ReplyAsync($"Music stream has been reset! Use {Format.Code($"{_config.DiscordConfig.Prefix}music join")} to create a new music stream.");
			}

			[Command("playlist")]
			public async Task PlaylistAsync()
			{
				var data = Context.Database.ServerMusics.GetData(Context.Guild.Id);

				if (data == null || data.PlaylistId == ObjectId.Empty && !_full) {
					await ReplyAsync("Server playlist is already empty.");

					return;
				}

				await StreamAsync();
				await _musicService.Playlist.DeleteAsync(data.PlaylistId);

				data.PlaylistId = ObjectId.Empty;

				if (!_full) await ReplyAsync("Server playlist is now empty.");
			}

			[Command("full")]
			public async Task FullAsync()
			{
				_full = true;

				await PlaylistAsync();

				var data = Context.Database.ServerMusics.GetData(Context.Guild.Id);

				if (data == null) {
					await ReplyAsync("Music has no data to reset.");

					return;
				}

				Context.Database.ServerMusics.Remove(data);

				await ReplyAsync("Music settings & playlist has been reset.");
			}
		}
	}
}