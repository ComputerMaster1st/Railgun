using System.Threading.Tasks;
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
		public class MusicAuto : SystemBase
		{
			private readonly PlayerController _playerController;

			public MusicAuto(PlayerController playerController) => _playerController = playerController;

			[Command("join")]
			public Task JoinAsync()
			{
				var data = Context.Database.ServerMusics.GetOrCreateData(Context.Guild.Id);
				var vc = (Context.Author as IGuildUser).VoiceChannel;

				if (vc == null && data.AutoVoiceChannel == 0)
					return ReplyAsync("Music Auto-Join is currently disabled. Please join a voice channel and run this command again to enable it.");
				if (vc == null && data.AutoVoiceChannel != 0) {
					data.AutoVoiceChannel = 0;
					data.AutoTextChannel = 0;
					return ReplyAsync("Music Auto-Join has been disabled.");
				}

				data.AutoVoiceChannel = vc.Id;
				data.AutoTextChannel = Context.Channel.Id;

				return ReplyAsync($"{(data.AutoVoiceChannel == 0 ? "Music Auto-Join is now enabled!" : "")} Will automatically join {Format.Bold(vc.Name)} and use {Format.Bold("#" + Context.Channel.Name)} to post status messages.");
			}

			[Command("skip")]
			public Task SkipAsync()
			{
				var data = Context.Database.ServerMusics.GetOrCreateData(Context.Guild.Id);
				data.AutoSkip = !data.AutoSkip;
				return ReplyAsync($"Music Auto-Skip is now {Format.Bold(data.AutoSkip ? "Enabled" : "Disabled")}.");
			}

			[Command("download")]
			public Task DownloadAsync()
			{
				var data = Context.Database.ServerMusics.GetOrCreateData(Context.Guild.Id);
				data.AutoDownload = !data.AutoDownload;
				return ReplyAsync($"Music Auto-Download is now {Format.Bold(data.AutoDownload ? "Enabled" : "Disabled")}.");
			}

			[Command("loop")]
			public Task AutoLoopAsync()
			{
				var data = Context.Database.ServerMusics.GetOrCreateData(Context.Guild.Id);
				data.PlaylistAutoLoop = !data.PlaylistAutoLoop;

				var container = _playerController.GetPlayer(Context.Guild.Id);
				if (container != null) container.Player.PlaylistAutoLoop = data.PlaylistAutoLoop;

				return ReplyAsync($"Music Playlist Auto-Loop is now {Format.Bold(data.PlaylistAutoLoop ? "Enabled" : "Disabled")}.");
			}
		}
	}
}