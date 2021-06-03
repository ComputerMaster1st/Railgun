using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.AntiCaps
{
    public partial class AntiCaps
    {
		[Alias("minlength")]
        public class AntiCapsMinLength : SystemBase
        {
			[Command]
			public Task ExecuteAsync(int length)
			{
				if (length < 0)
					return ReplyAsync("Please specify a minimum message length of 0 or above.");

				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
				var data = profile.Filters.Caps;

				data.Length = length;

				if (!data.IsEnabled) 
					data.IsEnabled = true;

				return ReplyAsync($"Anti-Caps is now set to scan messages longer than {Format.Bold(length.ToString())} characters.");
			}
		}
    }
}
