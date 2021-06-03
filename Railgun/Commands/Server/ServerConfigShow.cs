using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Server
{
    public partial class Server
    {
        public partial class ServerConfig
        {
			[Alias("show")]
            public class ServerConfigShow : SystemBase
            {
				[Command]
				public Task ExecuteAsync()
				{
					var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
					var command = profile.Command;
					var mention = profile.Globals;
					var output = new StringBuilder()
						.AppendLine("Railgun Server Configuration").AppendLine()
						.AppendFormat("    Server Name : {0}", Context.Guild.Name).AppendLine()
						.AppendFormat("      Server ID : {0}", Context.Guild.Id).AppendLine().AppendLine()
						.AppendFormat("     Delete CMD : {0}", command != null && command.DeleteCmdAfterUse ? "Yes" : "No").AppendLine()
						.AppendFormat("Respond To Bots : {0}", command != null && command.RespondToBots ? "Yes" : "No").AppendLine()
						.AppendFormat("  Allow Mention : {0}", mention != null && mention.DisableMentions ? "No" : "Yes").AppendLine()
						.AppendFormat("  Server Prefix : {0}", command != null && !string.IsNullOrEmpty(command.Prefix) ? command.Prefix : "Not Set").AppendLine();

					return ReplyAsync(Format.Code(output.ToString()));
				}
			}
        }
    }
}
