using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;

namespace Railgun.Commands.Server
{
	public partial class Server
	{
		[Alias("config"), UserPerms(GuildPermission.ManageGuild)]
		public partial class ServerConfig : SystemBase
		{
			[Command("mention")]
			public Task MentionAsync()
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Globals;

				data.DisableMentions = !data.DisableMentions;
				return ReplyAsync($"Server mentions are now {(data.DisableMentions ? Format.Bold("Enabled") : Format.Bold("Disabled"))}.");
			}
		}
	}
}