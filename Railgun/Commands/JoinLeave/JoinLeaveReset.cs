using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.JoinLeave
{
    public partial class JoinLeave
    {
		[Alias("reset")]
        public class JoinLeaveReset : SystemBase
        {
			[Command]
			public Task ExecuteAsync()
			{
				var data = Context.Database.ServerProfiles.GetData(Context.Guild.Id);

				if (data == null)
					return ReplyAsync("Join/Leave Notifications has no data to reset.");

				data.ResetJoinLeave();

				return ReplyAsync("Join/Leave Notifications has been reset & disabled.");
			}
		}
    }
}
