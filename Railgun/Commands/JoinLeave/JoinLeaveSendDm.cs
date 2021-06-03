using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.JoinLeave
{
    public partial class JoinLeave
    {
        [Alias("sendtodm")]
        public class JoinLeaveSendDm : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
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
}
