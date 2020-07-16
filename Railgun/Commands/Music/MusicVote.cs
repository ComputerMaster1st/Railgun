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
		[Alias("vote"), UserPerms(GuildPermission.ManageGuild)]
		public class MusicVote : SystemBase
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
			
			[Command]
			public Task EnableAsync()
			{
				var data = GetData(Context.Guild.Id, true);
				data.VoteSkipEnabled = !data.VoteSkipEnabled;
				return ReplyAsync($"Music Vote-Skip is now {(data.VoteSkipEnabled ? Format.Bold($"Enabled @ {data.VoteSkipLimit}%") : Format.Bold("Disabled"))}.");
			}

			[Command("percent")]
			public Task SetPercentAsync(int percent)
			{
				if (percent < 10 || percent > 100)
					return ReplyAsync("Percentage must be set between 10-100.");

				var data = GetData(Context.Guild.Id, true);
				data.VoteSkipLimit = percent;

				if (!data.VoteSkipEnabled) data.VoteSkipEnabled = true;
				return ReplyAsync($"Music Vote-Skip is now {(!data.VoteSkipEnabled ? Format.Bold("enabled &") : "")} set to skip songs when {data.VoteSkipLimit}% of users have voted.");
			}
		}
	}
}