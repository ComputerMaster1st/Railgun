using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;
using TreeDiagram.Enums;
using TreeDiagram.Models;
using TreeDiagram.Models.Server;

namespace Railgun.Commands.JoinLeave
{
	public partial class JoinLeave : SystemBase
	{
		[Alias("add", "+")]
		public class JoinLeaveAdd : SystemBase
		{
			private ServerJoinLeave GetData(ulong guildId, bool create = false)
			{
				ServerProfile data;

				if (create)
					data = Context.Database.ServerProfiles.GetOrCreateData(guildId);
				else {
					data = Context.Database.ServerProfiles.GetData(guildId);

					if (data == null) 
						return null;
				}

				if (data.JoinLeave == null)
					if (create)
						data.JoinLeave = new ServerJoinLeave();
				
				return data.JoinLeave;
			}

			private Task MsgHandlerAsync(string msg, MsgType type)
			{
				if (string.IsNullOrWhiteSpace(msg)) return ReplyAsync("Please specify a message to add.");

				var data = GetData(Context.Guild.Id, true);

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