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
using TreeDiagram.Models;
using TreeDiagram.Models.Server;

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

			private ServerMusic GetData(ulong guildId, bool create = false)
			{
				ServerProfile data;

				if (create)
					data = Context.Database.ServerProfiles.GetOrCreateData(guildId);
				else {
					data = Context.Database.ServerProfiles.GetData(guildId);

					if (data == null) 
						return null;
				}

				if (data.Music == null)
					if (create)
						data.Music = new ServerMusic();
				
				return data.Music;
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
				var data = GetData(Context.Guild.Id);

				if (data == null || data.PlaylistId == ObjectId.Empty && !_full) {
					await ReplyAsync("Server playlist is already empty.");
					return;
				}

				await StreamAsync();
				await SystemUtilities.DeletePlaylistAsync(_musicService, data.PlaylistId);

				data.PlaylistId = ObjectId.Empty;

				if (!_full) await ReplyAsync("Server playlist is now empty.");
			}

			[Command("full")]
			public async Task FullAsync()
			{
				_full = true;

				await PlaylistAsync();

				var data = Context.Database.ServerProfiles.GetData(Context.Guild.Id);

				if (data == null || data.Music == null) {
					await ReplyAsync("Music has no data to reset.");
					return;
				}

				data.Music = null;
				await ReplyAsync("Music settings & playlist has been reset.");
			}
		}
	}
}