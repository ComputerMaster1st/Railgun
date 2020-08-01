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
			var profile = Context.Database.UserProfiles.GetOrCreateData(Context.Author.Id);
            var data = profile.Globals;

			data.DisableMentions = !data.DisableMentions;
			return ReplyAsync($"Personal mentions are now {(data.DisableMentions ? Format.Bold("Enabled") : Format.Bold("Disabled"))}.");
		}

		[Command("prefix")]
		public Task PrefixAsync([Remainder] string input = null)
		{
			var profile = Context.Database.UserProfiles.GetOrCreateData(Context.Author.Id);
            var data = profile.Globals;

			if (string.IsNullOrWhiteSpace(input) && string.IsNullOrWhiteSpace(data.Prefix))
				return ReplyAsync("No prefix has been specified. Please specify a prefix.");
			if (string.IsNullOrWhiteSpace(input) && !string.IsNullOrWhiteSpace(data.Prefix)) {
				data.Prefix = string.Empty;
				return ReplyAsync("Personal prefix has been removed.");
			}

			data.Prefix = input;
			return ReplyAsync($"Personal prefix has been set! {Format.Code(input + " <command>")}!");
		}

		[Command("show")]
		public Task ShowAsync()
		{
			var profile = Context.Database.UserProfiles.GetOrCreateData(Context.Author.Id);
            var data = profile.Globals;
			var output = new StringBuilder()
				.AppendLine("Railgun User Configuration:").AppendLine()
				.AppendFormat("       Username : {0}#{1}", Context.Author.Username, Context.Author.DiscriminatorValue).AppendLine()
				.AppendFormat("        User ID : {0}", Context.Author.Id).AppendLine().AppendLine()
				.AppendFormat("  Allow Mention : {0}", data.DisableMentions ? "No" : "Yes").AppendLine()
				.AppendFormat("Personal Prefix : {0}", !string.IsNullOrWhiteSpace(data.Prefix) ? data.Prefix : "Not Set").AppendLine();

			return ReplyAsync(Format.Code(output.ToString()));
		}
	}
}