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
		[Alias("reset"), UserPerms(GuildPermission.ManageMessages)]
        public class RstReset : SystemBase
        {
			[Command]
			public Task ExecuteAsync()
			{
				var data = Context.Database.ServerProfiles.GetData(Context.Guild.Id);

				if (data == null)
					return ReplyAsync("RST has no data to reset.");

				data.Fun.ResetRst();
				return ReplyAsync("RST has been reset.");
			}
		}
    }
}
