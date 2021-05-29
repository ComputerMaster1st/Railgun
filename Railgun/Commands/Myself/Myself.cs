using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;

namespace Railgun.Commands.Myself
{
	[Alias("myself", "self")]
	public partial class Myself : SystemBase
	{
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