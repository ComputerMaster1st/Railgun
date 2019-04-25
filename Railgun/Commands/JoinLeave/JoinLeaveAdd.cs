using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;
using TreeDiagram.Enums;

namespace Railgun.Commands.JoinLeave
{
	public partial class JoinLeave : SystemBase
	{
		[Alias("add")]
		public class JoinLeaveAdd : SystemBase
		{
			private async Task MsgHandlerAsync(string msg, MsgType type)
			{
				if (string.IsNullOrWhiteSpace(msg)) {
					await ReplyAsync("Please specify a message to add.");

					return;
				}

				var data = Context.Database.ServerJoinLeaves.GetOrCreateData(Context.Guild.Id);

				if ((type == MsgType.Join && data.JoinMessages.Contains(msg)) || (type == MsgType.Leave && data.LeaveMessages.Contains(msg))) {
					await ReplyAsync("Specified message is already listed.");

					return;
				}

				data.AddMessage(msg, type);

				await ReplyAsync($"Successfully added {Format.Code(msg)} to {Format.Bold(type.ToString())} message.");
			}

			[Command("joinmsg")]
			public Task JoinAsync([Remainder] string msg) => MsgHandlerAsync(msg, MsgType.Join);

			[Command("leavemsg")]
			public Task LeaveAsync([Remainder] string msg) => MsgHandlerAsync(msg, MsgType.Leave);
		}
	}
}