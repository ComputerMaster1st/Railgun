using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using TreeDiagram;
using TreeDiagram.Enums;
using TreeDiagram.Models;
using TreeDiagram.Models.Server;

namespace Railgun.Commands.JoinLeave
{
	public partial class JoinLeave : SystemBase
	{
		[Alias("remove", "-")]
		public class JoinLeaveRemove : SystemBase
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
			
			private Task MsgHandlerAsync(int index, MsgType type)
			{
				if (index < 0) return ReplyAsync("The specified Id can not be lower than 0.");

				var data = GetData(Context.Guild.Id);

				if (data == null) return ReplyAsync("Join/Leave has yet to be configured.");
				if ((type == MsgType.Join && data.JoinMessages.Count <= index) || (type == MsgType.Leave && data.LeaveMessages.Count <= index))
					return ReplyAsync("Specified message is not listed.");

				data.RemoveMessage(index, type);
				return ReplyAsync($"Successfully removed from {Format.Bold(type.ToString())} messages.");
			}

			[Command("joinmsg")]
			public Task JoinAsync(int msg) => MsgHandlerAsync(msg, MsgType.Join);

			[Command("leavemsg")]
			public Task LeaveAsync(int msg) => MsgHandlerAsync(msg, MsgType.Leave);
		}
	}
}