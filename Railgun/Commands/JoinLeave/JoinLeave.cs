using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Extensions;
using TreeDiagram;

namespace Railgun.Commands.JoinLeave
{
	[Alias("joinleave"), UserPerms(GuildPermission.ManageGuild)]
	public partial class JoinLeave : SystemBase
	{
		[Command]
		public async Task EnableAsync()
		{
			var data = Context.Database.ServerJoinLeaves.GetOrCreateData(Context.Guild.Id);

			if (data.ChannelId == Context.Channel.Id) {
				data.ChannelId = 0;

				await ReplyAsync($"Join/Leave Notifications is now {Format.Bold("Disabled")}.");

				return;
			}

			data.ChannelId = Context.Channel.Id;

			await ReplyAsync($"Join/Leave Notifications is now {Format.Bold((data.ChannelId == 0 ? "Enabled & Set" : "Set"))} to this channel.");
		}

		[Command("deleteafter"), BotPerms(ChannelPermission.ManageMessages)]
		public async Task DeleteAfterAsync(int minutes = 0)
		{
			if (minutes < 0) {
				await ReplyAsync("Minutes can not be less than 0.");

				return;
			}

			var data = Context.Database.ServerJoinLeaves.GetOrCreateData(Context.Guild.Id);

			if (minutes == 0 && data.DeleteAfterMinutes == 0) {
				await ReplyAsync("Already set to not delete Join/Leave notifications.");

				return;
			} else if (minutes == 0 && data.DeleteAfterMinutes != 0) {
				data.DeleteAfterMinutes = 0;

				await ReplyAsync("No longer deleting Join/Leave notifications.");

				return;
			}

			data.DeleteAfterMinutes = minutes;

			await ReplyAsync($"Join/Leave notifications will now be deleted after {minutes} minutes.");
		}

		[Command("sendtodm")]
		public async Task DmAsync()
		{
			var data = Context.Database.ServerJoinLeaves.GetData(Context.Guild.Id);

			if (data == null || data.ChannelId == 0) {
				await ReplyAsync("Join/Leave Notifications is currently turned off. Please turn on before using this command.");

				return;
			}

			data.SendToDM = !data.SendToDM;

			await ReplyAsync($"Join/Leave Notification will {Format.Bold((data.SendToDM ? "Now" : "No Longer"))} be sent via DMs.");
		}

		[Command("show"), BotPerms(ChannelPermission.AttachFiles)]
		public async Task ShowAsync()
		{
			var data = Context.Database.ServerJoinLeaves.GetData(Context.Guild.Id);

			if (data == null) {
				await ReplyAsync("Join/Leave Notifications has not been configured!");

				return;
			}

			var tc = data.ChannelId != 0 ? await Context.Guild.GetTextChannelAsync(data.ChannelId) : null;
			var tempString = string.Empty;
			var output = new StringBuilder();

			if (data.SendToDM) tempString = "Private Messages";
			else if (tc != null) tempString = tc.Name;
			else tempString = "TextChannel not set or missing?";

			output.AppendLine(Format.Bold("Join/Leave Notifications")).AppendLine()
				.AppendFormat("Send To : {0}", Format.Bold(tempString)).AppendLine().AppendLine()
				.AppendLine(Format.Bold("Join Messages :")).AppendLine();

			data.JoinMessages.ForEach(msg => output.AppendFormat("[{0}] : {1}", Format.Code(data.JoinMessages.IndexOf(msg).ToString()), msg).AppendLine());

			output.AppendLine().AppendLine(Format.Bold("Leave Messages :")).AppendLine();

			data.LeaveMessages.ForEach(msg => output.AppendFormat("[{0}] : {1}", Format.Code(data.LeaveMessages.IndexOf(msg).ToString()), msg).AppendLine());

			if (output.Length <= 1950) {
				await ReplyAsync(output.ToString());

				return;
			}

			await ((ITextChannel)Context.Channel).SendStringAsFileAsync("JoinLeave Notifications.txt", output.ToString());
		}

		[Command("reset")]
		public async Task ResetAsync()
		{
			var data = Context.Database.ServerJoinLeaves.GetData(Context.Guild.Id);

			if (data == null) {
				await ReplyAsync("Join/Leave Notifications has no data to reset.");

				return;
			}

			Context.Database.ServerJoinLeaves.Remove(data);

			await ReplyAsync("Join/Leave Notifications has been reset & disabled.");
		}
	}
}