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

		[Command("show")]
		public async Task ShowAsync()
		{
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Music;
			var songCount = 0;

			if (data.PlaylistId != ObjectId.Empty) {
				var playlist = await SystemUtilities.GetPlaylistAsync(_musicService, data);
				songCount = playlist.Songs.Count;
			}

			var npTc = data.NowPlayingChannel != 0 ? await Context.Guild.GetTextChannelAsync(data.NowPlayingChannel) : null;
			var autoJoinOutput = new StringBuilder();
			var badAutoJoinConfigs = new List<MusicAutoJoinConfig>();

			foreach (var autoJoinConfig in data.AutoJoinConfigs)
			{
				var vc = await Context.Guild.GetVoiceChannelAsync(autoJoinConfig.VoiceChannelId);
				var tc = await Context.Guild.GetTextChannelAsync(autoJoinConfig.TextChannelId);

				if (vc == null) {
					badAutoJoinConfigs.Add(autoJoinConfig);
					continue;
				}

				autoJoinOutput.AppendFormat("| {0}:{1} |", vc.Name, (npTc == null ? (tc == null ? "[UNKNOWN TEXT CHANNEL]" : tc.Name) : "[USING \"Now Playing\" CHANNEL]"));
			}

			foreach (var autoJoinConfig in badAutoJoinConfigs)
				data.AutoJoinConfigs.Remove(autoJoinConfig);

			if (data.AutoJoinConfigs.Count < 1) {
				autoJoinOutput.Clear();
				autoJoinOutput.AppendFormat("Not set.");
			}

			var autoDownloadOutput = data.AutoDownload ? "Enabled" : "Disabled";
			var autoSkipOutput = data.AutoSkip ? "Enabled" : "Disabled";
			var silentPlayingOutput = data.SilentNowPlaying ? "Enabled" : "Disabled";
			var silentInstallOutput = data.SilentSongProcessing ? "Enabled" : "Disabled";
			var npTcName = npTc != null ? $"#{npTc.Name}" : "None";
			var voteskipOutput = data.VoteSkipEnabled ? $"Enabled @ {data.VoteSkipLimit}% Users" : "Disabled";
			var autoPlay = string.IsNullOrWhiteSpace(data.AutoPlaySong) ? "Disabled" : data.AutoPlaySong;
			var autoLoop = data.PlaylistAutoLoop ? "Enabled" : "Disabled";
			var whitelist = data.WhitelistMode ? "Enabled" : "Disabled";
			var output = new StringBuilder();
			var roleLock = new StringBuilder();

			if (data.AllowedRoles.Count > 0)
				foreach (var allowedRole in data.AllowedRoles) {
					var role = Context.Guild.GetRole(allowedRole);
					roleLock.AppendFormat("| {0} |", role.Name);
				} else roleLock.AppendFormat("None.");

			output.AppendLine("Music Settings")
				.AppendLine()
				.AppendFormat("   Number Of Songs : {0}", songCount).AppendLine()
				.AppendLine()
				.AppendFormat("         Auto-Join : {0}", autoJoinOutput).AppendLine()
				.AppendFormat("     Auto-Download : {0}", autoDownloadOutput).AppendLine()
				.AppendFormat("         Auto-Skip : {0}", autoSkipOutput).AppendLine()
				.AppendFormat("         Auto-Play : {0}", autoPlay).AppendLine()
				.AppendFormat("Playlist Auto-Loop : {0}", autoLoop).AppendLine()
				.AppendLine()
				.AppendFormat("    Silent Running : {0}", silentPlayingOutput).AppendLine()
				.AppendFormat("    Silent Install : {0}", silentInstallOutput).AppendLine()
				.AppendLine()
				.AppendFormat("   NP Dedi Channel : {0}", npTcName).AppendLine()
				.AppendFormat("         Vote-Skip : {0}", voteskipOutput).AppendLine()
				.AppendFormat("       Role-Locked : {0}", roleLock.ToString()).AppendLine()
				.AppendFormat("      Whitelisting : {0}", whitelist);

			await ReplyAsync(Format.Code(output.ToString()));
		}
	}
}