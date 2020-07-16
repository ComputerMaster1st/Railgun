using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;
using TreeDiagram.Models;
using TreeDiagram.Models.Server;

namespace Railgun.Commands.Music
{
	public partial class Music
	{
		[Alias("silent"), UserPerms(GuildPermission.ManageGuild)]
		public class MusicSilent : SystemBase
		{
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
			
			[Command("running")]
			public Task RunningAsync()
			{
				var data = GetData(Context.Guild.Id, true);
				data.SilentNowPlaying = !data.SilentNowPlaying;
				return ReplyAsync($"{Format.Bold(data.SilentNowPlaying ? "Engaged" : "Disengaged")} Silent Running!");
			}

			[Command("install")]
			public Task InstallAsync()
			{
				var data = GetData(Context.Guild.Id, true);
				data.SilentSongProcessing = !data.SilentSongProcessing;
				return ReplyAsync($"{Format.Bold(data.SilentSongProcessing ? "Engaged" : "Disengaged")} Silent Installation!");
			}
		}
	}
}