using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Linq;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.AntiCaps
{
    public partial class AntiCaps
    {
		[Alias("ignore")]
        public class AntiCapsIgnore : SystemBase
        {
			[Command]
			public Task ExecuteAsync(ITextChannel tc)
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
				var data = profile.Filters.Caps;

				if (data.IgnoredChannels.Any(f => f == tc.Id))
				{
					data.RemoveIgnoreChannel(tc.Id);
					return ReplyAsync("Anti-Caps is now monitoring this channel.");
				}
				else
				{
					data.AddIgnoreChannel(tc.Id);
					return ReplyAsync("Anti-Caps is no longer monitoring this channel.");
				}
			}

			[Command]
			public Task ExecuteAsync()
				=> ExecuteAsync(Context.Channel as ITextChannel);
		}
    }
}
