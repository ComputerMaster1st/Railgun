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
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Music;
				data.SilentNowPlaying = !data.SilentNowPlaying;
				return ReplyAsync($"{Format.Bold(data.SilentNowPlaying ? "Engaged" : "Disengaged")} Silent Running!");
			}

			[Command("install")]
			public Task InstallAsync()
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Music;
				data.SilentSongProcessing = !data.SilentSongProcessing;
				return ReplyAsync($"{Format.Bold(data.SilentSongProcessing ? "Engaged" : "Disengaged")} Silent Installation!");
			}
		}
	}
}