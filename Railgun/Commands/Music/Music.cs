using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Finite.Commands;
using MongoDB.Bson;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Configuration;
using Railgun.Core.Enums;
using Railgun.Music;
using TreeDiagram;
using TreeDiagram.Models.SubModels;

namespace Railgun.Commands.Music
{
    [Alias("music", "m"), RoleLock(ModuleType.Music)]
	public partial class Music : SystemBase
	{
		private readonly MasterConfig _config;
		private readonly PlayerController _playerController;
		private readonly MusicService _musicService;

		public Music(MasterConfig config, PlayerController playerController, MusicService musicService)
		{
			_config = config;
			_playerController = playerController;
			_musicService = musicService;
		}

		[Command("join"), BotPerms(GuildPermission.Connect | GuildPermission.Speak)]
		public Task JoinAsync()
		{
			if (_playerController.GetPlayer(Context.Guild.Id) != null)
				return ReplyAsync($"Sorry, I'm already in a voice channel. If you're experiencing problems, please do {Format.Code($"{_config.DiscordConfig.Prefix}music reset stream.")}");

			var user = (IGuildUser)Context.Author;
			var vc = user.VoiceChannel;

			if (vc == null) return ReplyAsync("Please go into a voice channel before inviting me.");

			return _playerController.CreatePlayerAsync(user, vc, (ITextChannel)Context.Channel);
		}

        [Command("repeat")]
        public Task RepeatAsync() => RepeatAsync(1);

		[Command("repeat")]
		public Task RepeatAsync(int count)
		{
			var container = _playerController.GetPlayer(Context.Guild.Id);

			if (container == null) return ReplyAsync("I'm not playing anything at this time.");

			var player = container.Player;
			player.RepeatSong = count;

			return ReplyAsync("Repeating song after finishing.");
		}

		[Command("ping")]
		public Task PingAsync()
		{
			var container = _playerController.GetPlayer(Context.Guild.Id);
			return ReplyAsync(container == null ? "Can not check ping due to not being in voice channel." : $"Ping to Discord Voice: {Format.Bold(container.Player.Latency.ToString())}ms");
		}

		[Command("whitelist")]
		public Task WhitelistAsync()
        {
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Music;
			data.WhitelistMode = !data.WhitelistMode;
			return ReplyAsync($"Music Whitelist Mode is now {Format.Bold(data.WhitelistMode ? "Enabled" : "Disabled")}.");
        }
	}
}