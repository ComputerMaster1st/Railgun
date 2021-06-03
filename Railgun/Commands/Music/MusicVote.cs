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
		public partial class MusicVote : SystemBase
		{			
			[Command]
			public Task ExecuteAsync()
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Music;

				data.VoteSkipEnabled = !data.VoteSkipEnabled;

				return ReplyAsync($"Music Vote-Skip is now {(data.VoteSkipEnabled ? Format.Bold($"Enabled @ {data.VoteSkipLimit}%") : Format.Bold("Disabled"))}.");
			}
		}
	}
}