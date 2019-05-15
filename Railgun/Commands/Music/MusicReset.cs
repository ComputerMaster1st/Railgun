using System.Threading.Tasks;
using AudioChord;
using Discord;
using Finite.Commands;
using MongoDB.Bson;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Configuration;
using Railgun.Music;
using TreeDiagram;

namespace Railgun.Commands.Music
{
	public partial class Music
	{
		[Alias("reset"), UserPerms(GuildPermission.ManageGuild)]
		public class MusicReset : SystemBase
		{
			private readonly MasterConfig _config;
			private readonly PlayerController _playerController;
			private readonly MusicService _musicService;
			private bool _full;

			public MusicReset(MasterConfig config, PlayerController playerController, MusicService musicService)
			{
				_config = config;
				_playerController = playerController;
				_musicService = musicService;
			}

			[Command("stream")]
			public Task StreamAsync()
			{
				if (_playerController.GetPlayer(Context.Guild.Id) == null)
					if (!_full) return ReplyAsync("I'm not streaming any music at this time.");

				_playerController.DisconnectPlayer(Context.Guild.Id);
				if (!_full) return ReplyAsync($"Music stream has been reset! Use {Format.Code($"{_config.DiscordConfig.Prefix}music join")} to create a new music stream.");
				return Task.CompletedTask;
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