using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using TreeDiagram;
using TreeDiagram.Enums;

namespace Railgun.Commands.JoinLeave
{
	public partial class JoinLeave : SystemBase
	{
		[Alias("remove")]
		public class JoinLeaveRemove : SystemBase
		{
			private async Task MsgHandlerAsync(int index, MsgType type)
			{
				if (index < 0) {
					await ReplyAsync("The specified Id can not be lower than 0.");

					return;
				}

				var data = Context.Database.ServerJoinLeaves.GetData(Context.Guild.Id);

				if (data == null) {
					await ReplyAsync("Join/Leave has yet to be configured.");

					return;
				} else if ((type == MsgType.Join && data.JoinMessages.Count <= index) || (type == MsgType.Leave && data.LeaveMessages.Count <= index)) {
					await ReplyAsync("Specified message is not listed.");

					return;
				}

				data.RemoveMessage(index, type);

				await ReplyAsync($"Successfully removed from {Format.Bold(type.ToString())} messages.");
			}

			[Command("joinmsg")]
			public Task JoinAsync(int msg) => MsgHandlerAsync(msg, MsgType.Join);

			[Command("leavemsg")]
			public Task LeaveAsync(int msg) => MsgHandlerAsync(msg, MsgType.Leave);
		}
	}
}