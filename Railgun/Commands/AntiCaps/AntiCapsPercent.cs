using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.AntiCaps
{
    public partial class AntiCaps
    {
		[Alias("percent")]
        public class AntiCapsPercent : SystemBase
        {
			[Command]
			public Task ExecuteAsync(int percent)
			{
				if (percent < 50 || percent > 100)
					return ReplyAsync("Anti-Caps Percentage must be between 50-100.");

				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
				var data = profile.Filters.Caps;

				data.Percentage = percent;

				if (!data.IsEnabled) 
					data.IsEnabled = true;

				return ReplyAsync($"Anti-Caps is now set to trigger at {Format.Bold($"{percent}%")} sensitivity.");
			}
		}
    }
}
