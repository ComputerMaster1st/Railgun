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
		[Alias("add", "+")]
		public class JoinLeaveAdd : SystemBase
		{
			private Task MsgHandlerAsync(string msg, MsgType type)
			{
				if (string.IsNullOrWhiteSpace(msg)) return ReplyAsync("Please specify a message to add.");

				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.JoinLeave;

				if ((type == MsgType.Join && data.JoinMessages.Contains(msg)) || (type == MsgType.Leave && data.LeaveMessages.Contains(msg)))
					return ReplyAsync("Specified message is already listed.");

				data.AddMessage(msg, type);
				return ReplyAsync($"Successfully added {Format.Code(msg)} to {Format.Bold(type.ToString())} message.");
			}

			[Command("joinmsg")]
			public Task JoinAsync([Remainder] string msg) => MsgHandlerAsync(msg, MsgType.Join);

			[Command("leavemsg")]
			public Task LeaveAsync([Remainder] string msg) => MsgHandlerAsync(msg, MsgType.Leave);
		}
	}
}