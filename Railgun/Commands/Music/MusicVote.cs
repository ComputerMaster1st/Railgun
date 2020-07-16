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
		[Alias("vote"), UserPerms(GuildPermission.ManageGuild)]
		public class MusicVote : SystemBase
		{			
			[Command]
			public Task EnableAsync()
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Music;
				data.VoteSkipEnabled = !data.VoteSkipEnabled;
				return ReplyAsync($"Music Vote-Skip is now {(data.VoteSkipEnabled ? Format.Bold($"Enabled @ {data.VoteSkipLimit}%") : Format.Bold("Disabled"))}.");
			}

			[Command("percent")]
			public Task SetPercentAsync(int percent)
			{
				if (percent < 10 || percent > 100)
					return ReplyAsync("Percentage must be set between 10-100.");

				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Music;
				data.VoteSkipLimit = percent;

				if (!data.VoteSkipEnabled) data.VoteSkipEnabled = true;
				return ReplyAsync($"Music Vote-Skip is now {(!data.VoteSkipEnabled ? Format.Bold("enabled &") : "")} set to skip songs when {data.VoteSkipLimit}% of users have voted.");
			}
		}
	}
}