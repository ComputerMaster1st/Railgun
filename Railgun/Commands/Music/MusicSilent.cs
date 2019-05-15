using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;

namespace Railgun.Commands.Music
{
	public partial class Music
	{
		[Alias("silent"), UserPerms(GuildPermission.ManageGuild)]
		public class MusicSilent : SystemBase
		{
			[Command("running")]
			public Task RunningAsync()
			{
				var data = Context.Database.ServerMusics.GetOrCreateData(Context.Guild.Id);
				data.SilentNowPlaying = !data.SilentNowPlaying;
				return ReplyAsync($"{Format.Bold(data.SilentNowPlaying ? "Engaged" : "Disengaged")} Silent Running!");
			}

			[Command("install")]
			public Task InstallAsync()
			{
				var data = Context.Database.ServerMusics.GetOrCreateData(Context.Guild.Id);
				data.SilentSongProcessing = !data.SilentSongProcessing;
				return ReplyAsync($"{Format.Bold(data.SilentSongProcessing ? "Engaged" : "Disengaged")} Silent Installation!");
			}
		}
	}
}