using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Rst
{
    public partial class Rst
    {
		[Alias("allowdeny"), UserPerms(GuildPermission.ManageMessages)]
		public class RstAllowDeny : SystemBase
		{
			[Command]
			public Task ExecuteAsync()
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
				var data = profile.Fun.Rst;

				data.IsEnabled = !data.IsEnabled;

				return ReplyAsync(string.Format("RST is now {0}!",
					data.IsEnabled ? Format.Bold("enabled") : Format.Bold("disabled")));
			}
		}
	}
}
