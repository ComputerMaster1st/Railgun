using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Warning
{
    public partial class Warning
    {
		[Alias("mylist")]
        public class WarningMyList : SystemBase
        {
			[Command]
			public Task ExecuteAsync()
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
				var data = profile.Warning;

				if (data.Warnings.Count < 1)
					return ReplyAsync("There are currently no users with warnings.");

				var warnings = data.GetWarnings(Context.Author.Id);

				if (warnings == null || warnings.Count < 1)
					return ReplyAsync("You have no warnings to your name.");

				var output = new StringBuilder()
					.AppendFormat("You have been warned {0} time(s) for...", 
						Format.Bold(warnings.Count.ToString())).AppendLine()
					.AppendLine();

				warnings.ForEach(reason => output.AppendFormat("---- {0}", Format.Bold(reason)).AppendLine());

				return ReplyAsync(output.ToString());
			}
		}
    }
}
