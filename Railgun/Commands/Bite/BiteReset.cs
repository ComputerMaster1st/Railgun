using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Bite
{
    public partial class Bite
    {
		[Alias("reset"), UserPerms(GuildPermission.ManageMessages)]
        public class BiteReset : SystemBase
        {
			[Command]
			public Task ExecuteAsync()
			{
				var data = Context.Database.ServerProfiles.GetData(Context.Guild.Id);

				if (data == null || data.Fun.Bites == null)
					return ReplyAsync("Bites has no data to reset.");

				data.Fun.ResetBites();
				return ReplyAsync("Bites has been reset.");
			}
		}
    }
}
