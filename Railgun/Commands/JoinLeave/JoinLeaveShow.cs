using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Extensions;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.JoinLeave
{
    public partial class JoinLeave
    {
		[Alias("show"), BotPerms(ChannelPermission.AttachFiles)]
        public class JoinLeaveShow : SystemBase
        {
			[Command]
			public async Task ExecuteAsync()
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
				var data = profile.JoinLeave;
				var tc = data.ChannelId != 0 ? await Context.Guild.GetTextChannelAsync(data.ChannelId) : null;
				var tempString = string.Empty;
				var output = new StringBuilder();

				if (data.SendToDM) 
					tempString = "Private Messages";
				else if (tc != null) 
					tempString = tc.Name;
				else 
					tempString = "TextChannel not set or missing?";

				output.AppendLine(Format.Bold("Join/Leave Notifications")).AppendLine()
					.AppendFormat("Send To : {0}", Format.Bold(tempString)).AppendLine().AppendLine()
					.AppendLine(Format.Bold("Join Messages :")).AppendLine();

				data.JoinMessages.ForEach(msg => output.AppendFormat("[{0}] : {1}", Format.Code(data.JoinMessages.IndexOf(msg).ToString()), msg).AppendLine());

				output.AppendLine().AppendLine(Format.Bold("Leave Messages :")).AppendLine();

				data.LeaveMessages.ForEach(msg => output.AppendFormat("[{0}] : {1}", Format.Code(data.LeaveMessages.IndexOf(msg).ToString()), msg).AppendLine());

				if (output.Length <= 1950)
				{
					await ReplyAsync(output.ToString());

					return;
				}

				await (Context.Channel as ITextChannel).SendStringAsFileAsync("JoinLeave Notifications.txt", output.ToString());
			}
		}
    }
}
