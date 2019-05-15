using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;

namespace Railgun.Commands
{
	[Alias("myself", "self")]
	public class Myself : SystemBase
	{
		[Command("mention")]
		public Task MentionsAsync()
		{
			var data = Context.Database.UserMentions.GetOrCreateData(Context.Author.Id);

			if (data.DisableMentions) {
				Context.Database.UserMentions.Remove(data);
				return ReplyAsync($"Personal mentions are now {Format.Bold("Enabled")}.");
			}

			data.DisableMentions = !data.DisableMentions;
			return ReplyAsync($"Personal mentions are now {Format.Bold("Disabled")}.");
		}

		[Command("prefix")]
		public Task PrefixAsync([Remainder] string input = null)
		{
			var data = Context.Database.UserCommands.GetData(Context.Author.Id);

			if (string.IsNullOrWhiteSpace(input) && data == null)
				return ReplyAsync("No prefix has been specified. Please specify a prefix.");
			if (string.IsNullOrWhiteSpace(input) && data != null) {
				Context.Database.UserCommands.Remove(data);
				return ReplyAsync("Personal prefix has been removed.");
			}

			data = Context.Database.UserCommands.GetOrCreateData(Context.Author.Id);
			data.Prefix = input;
			return ReplyAsync($"Personal prefix has been set! {Format.Code(input + " <command>")}!");
		}

		[Command("show")]
		public Task ShowAsync()
		{
			var prefix = Context.Database.UserCommands.GetData(Context.Author.Id);
			var mention = Context.Database.UserMentions.GetOrCreateData(Context.Author.Id);
			var output = new StringBuilder()
				.AppendLine("Railgun User Configuration:").AppendLine()
				.AppendFormat("       Username : {0}#{1}", Context.Author.Username, Context.Author.DiscriminatorValue).AppendLine()
				.AppendFormat("        User ID : {0}", Context.Author.Id).AppendLine().AppendLine()
				.AppendFormat("  Allow Mention : {0}", mention != null ? "No" : "Yes").AppendLine()
				.AppendFormat("Personal Prefix : {0}", prefix != null ? prefix.Prefix : "Not Set").AppendLine();

			return ReplyAsync(Format.Code(output.ToString()));
		}
	}
}