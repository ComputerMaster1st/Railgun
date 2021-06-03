using System.Threading.Tasks;
using AudioChord;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Configuration;
using Railgun.Core.Enums;
using Railgun.Music;

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
	}
}