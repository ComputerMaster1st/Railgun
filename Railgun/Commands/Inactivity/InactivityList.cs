using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Extensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Inactivity
{
    public partial class Inactivity
    {
        [Alias("list"), BotPerms(ChannelPermission.AttachFiles)]
        public class InactivityList : SystemBase
        {
            [Command]
            public async Task ExecuteAsync()
            {
                var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                var data = profile.Inactivity;
                var users = await Context.Guild.GetUsersAsync();
                var inactiveUsers = users.Count((u) => u.RoleIds.Contains(data.InactiveRoleId));
                var output = new StringBuilder()
                    .AppendLine("Inactivity Monitor Report!")
                    .AppendLine()
                    .AppendFormat("Currently Monitoring      : {0}/{1} users", data.Users.Count, users.Count).AppendLine()
                    .AppendFormat("Whitelisted (Users/Roles) : {0}/{1}", data.UserWhitelist.Count, data.RoleWhitelist.Count).AppendLine()
                    .AppendFormat("Users Marked As Inactive  : {0}", inactiveUsers).AppendLine()
                    .AppendLine();

                if (inactiveUsers < 1)
                {
                    await ReplyAsync(Format.Code(output.ToString()));
                    return;
                }

                foreach (var user in users)
                {
                    var activityContainer = data.Users.FirstOrDefault(u => u.UserId == user.Id);

                    output.AppendFormat("Username : {0}#{1} {2} ID : {3} {2} Status : {4} {2} Last Active : {5}",
                        user.Username, user.DiscriminatorValue, SystemUtilities.GetSeparator, user.Id,
                        !user.RoleIds.Contains(data.InactiveRoleId) ? "Active" : "Inactive"
                        , activityContainer != null
                            ? activityContainer.LastActive.ToString()
                            : "UNKNOWN! (Something went wrong!)")
                        .AppendLine();
                }

                await ((ITextChannel)Context.Channel).SendStringAsFileAsync("Inactivity Report", output.ToString());
            }
        }
    }
}
