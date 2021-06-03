using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.AntiCaps
{
    public partial class AntiCaps
    {
		[Alias("reset")]
        public class AntiCapsReset : SystemBase
        {
			[Command]
			public Task ExecuteAsync()
			{
				var profile = Context.Database.ServerProfiles.GetData(Context.Guild.Id);

				if (profile == null)
					return ReplyAsync("Anti-Caps has no data to reset.");

				profile.Filters.ResetCaps();

				return ReplyAsync("Anti-Caps has been reset & disabled.");
			}
		}
    }
}
