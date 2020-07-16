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
		public class ServerConfig : SystemBase
		{
			[Command("mention")]
			public Task MentionAsync()
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Globals;

				data.DisableMentions = !data.DisableMentions;
				return ReplyAsync($"Server mentions are now {(data.DisableMentions ? Format.Bold("Enabled") : Format.Bold("Disabled"))}.");
			}

			[Command("prefix")]
			public Task PrefixAsync([Remainder] string input = null)
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Command;

				if (string.IsNullOrWhiteSpace(input) && string.IsNullOrEmpty(data.Prefix))
					return ReplyAsync("No prefix has been specified. Please specify a prefix.");
				if (string.IsNullOrWhiteSpace(input) && !string.IsNullOrEmpty(data.Prefix)) {
					data.Prefix = string.Empty;
					return ReplyAsync("Server prefix has been removed.");
				}

				data.Prefix = input;
				return ReplyAsync($"Server prefix has been set! {Format.Code(input = "<command>")}!");
			}

			[Command("deletecmd"), BotPerms(GuildPermission.ManageMessages)]
			public Task DeleteCmdAsync()
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Command;

				data.DeleteCmdAfterUse = !data.DeleteCmdAfterUse;
				return ReplyAsync($"Commands used will {Format.Bold(data.DeleteCmdAfterUse ? "now" : "no longer")} be deleted.");
			}

			[Command("respondtobots")]
			public Task RespondAsync()
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Command;

				data.RespondToBots = !data.RespondToBots;
				return ReplyAsync($"I will {Format.Bold(data.RespondToBots ? "now" : "no longer")} respond to other bots.");
			}

			[Command("ignoreoldmsgs")]
			public Task IgnoreOldMessagesAsync()
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Command;

				data.IgnoreModifiedMessages = !data.IgnoreModifiedMessages;
				return ReplyAsync($"I will {Format.Bold(data.IgnoreModifiedMessages ? "now" : "no longer")} ignore modified messages. This includes pinned messages from now on.");
			}

			[Command("show")]
			public Task ShowAsync()
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