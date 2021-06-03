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
	}
}