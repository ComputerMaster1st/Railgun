using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.AntiUrl
{
    public partial class AntiUrl
    {
		[Alias("reset")]
		public class AntiUrlReset : SystemBase
		{
			[Command]
			public Task ExecuteAsync()
			{
				var data = Context.Database.ServerProfiles.GetData(Context.Guild.Id);

				if (data == null)
					return ReplyAsync("Anti-Url has no data to reset.");

				data.Filters.ResetUrls();

				return ReplyAsync("Anti-Url has been reset & disabled.");
			}
		}
	}
}
