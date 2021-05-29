using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Myself
{
    public partial class Myself
    {
        [Alias("prefix")]
        public class MyselfPrefix : SystemBase
        {
			[Command]
			public Task ExecuteAsync([Remainder] string input)
			{
				var profile = Context.Database.UserProfiles.GetOrCreateData(Context.Author.Id);
				var data = profile.Globals;

				data.Prefix = input;

				return ReplyAsync(string.Format("Personal prefix has been set! {0}!",
					Format.Code(input + " <command>")));
			}

			[Command]
			public Task ExecuteAsync()
            {
				var profile = Context.Database.UserProfiles.GetOrCreateData(Context.Author.Id);
				var data = profile.Globals;

				if (string.IsNullOrWhiteSpace(data.Prefix))
					return ReplyAsync("No prefix has been specified. Please specify a prefix.");

				data.Prefix = string.Empty;

				return ReplyAsync("Personal prefix has been removed.");
			}
		}
    }
}
