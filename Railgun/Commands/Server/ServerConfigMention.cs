using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Server
{
    public partial class Server
    {
        public partial class ServerConfig
        {
			[Alias("mention")]
			public class ServerConfigMention : SystemBase
			{
				[Command]
				public Task ExecuteAsync()
				{
					var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
					var data = profile.Globals;

					data.DisableMentions = !data.DisableMentions;

					return ReplyAsync($"Server mentions are now {(data.DisableMentions ? Format.Bold("Enabled") : Format.Bold("Disabled"))}.");
				}
			}
		}
    }
}
