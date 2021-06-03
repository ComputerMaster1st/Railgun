using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.JoinLeave
{
    public partial class JoinLeave
    {
		[Alias("deleteafter"), BotPerms(ChannelPermission.ManageMessages)]
        public class JoinLeaveDeleteAfter : SystemBase
        {
			[Command]
			public Task ExecuteAsync(int minutes)
			{
				if (minutes < 0) 
					return ReplyAsync("Minutes can not be less than 0.");

				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
				var data = profile.JoinLeave;

				if (minutes == 0 && data.DeleteAfterMinutes == 0)
					return ReplyAsync("Already set to not delete Join/Leave notifications.");
				
				if (minutes == 0 && data.DeleteAfterMinutes != 0)
				{
					data.DeleteAfterMinutes = 0;

					return ReplyAsync("No longer deleting Join/Leave notifications.");
				}

				data.DeleteAfterMinutes = minutes;

				return ReplyAsync($"Join/Leave notifications will now be deleted after {minutes} minutes.");
			}

			[Command]
			public Task ExecuteAsync()
				=> ExecuteAsync(0);
		}
    }
}
