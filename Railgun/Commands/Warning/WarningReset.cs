using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Warning
{
	public partial class Warning
	{
		[Alias("reset"), UserPerms(GuildPermission.ManageGuild)]
        public class WarningReset : SystemBase
        {
			[Command]
			public Task ExecuteAsync()
			{
				var data = Context.Database.ServerProfiles.GetData(Context.Guild.Id);

				if (data == null)
					return ReplyAsync("Warnings has no data to reset.");

				data.ResetWarning();

				return ReplyAsync("Warnings has been reset & disabled.");
			}
		}
    }
}
