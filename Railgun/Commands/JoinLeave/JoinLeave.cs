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
	[Alias("joinleave", "jl"), UserPerms(GuildPermission.ManageGuild)]
	public partial class JoinLeave : SystemBase
	{
		[Command]
		public Task EnableAsync()
		{
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.JoinLeave;

			if (data.ChannelId == Context.Channel.Id) {
				data.ChannelId = 0;
				return ReplyAsync($"Join/Leave Notifications is now {Format.Bold("Disabled")}.");
			}

			data.ChannelId = Context.Channel.Id;
			return ReplyAsync($"Join/Leave Notifications is now {Format.Bold((data.ChannelId == 0 ? "Enabled & Set" : "Set"))} to this channel.");
		}

		[Command("deleteafter"), BotPerms(ChannelPermission.ManageMessages)]
		public Task DeleteAfterAsync(int minutes = 0)
		{
			if (minutes < 0) return ReplyAsync("Minutes can not be less than 0.");

			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.JoinLeave;

			if (minutes == 0 && data.DeleteAfterMinutes == 0) {
				return ReplyAsync("Already set to not delete Join/Leave notifications.");
			} else if (minutes == 0 && data.DeleteAfterMinutes != 0) {
				data.DeleteAfterMinutes = 0;
				return ReplyAsync("No longer deleting Join/Leave notifications.");
			}

			data.DeleteAfterMinutes = minutes;
			return ReplyAsync($"Join/Leave notifications will now be deleted after {minutes} minutes.");
		}

		[Command("sendtodm")]
		public Task DmAsync()
		{
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.JoinLeave;

			if (data.ChannelId == 0) 
				return ReplyAsync("Join/Leave Notifications is currently turned off. Please turn on before using this command.");

			data.SendToDM = !data.SendToDM;
			return ReplyAsync($"Join/Leave Notification will {Format.Bold((data.SendToDM ? "Now" : "No Longer"))} be sent via DMs.");
		}

		[Command("show"), BotPerms(ChannelPermission.AttachFiles)]
		public async Task ShowAsync()
		{
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.JoinLeave;
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

			await (Context.Channel as ITextChannel).SendStringAsFileAsync("JoinLeave Notifications.txt", output.ToString());
		}

		[Command("reset")]
		public Task ResetAsync()
		{
			var data = Context.Database.ServerProfiles.GetData(Context.Guild.Id);

			if (data == null)
				return ReplyAsync("Join/Leave Notifications has no data to reset.");

			data.ResetJoinLeave();
			return ReplyAsync("Join/Leave Notifications has been reset & disabled.");
		}
	}
}