using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Inactivity
{
    public partial class Inactivity
    {
        [Alias("show")]
        public class InactivityShow : SystemBase
        {
            [Command]
            public async Task ExecuteAsync()
            {
                var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                var data = profile.Inactivity;

                var whitelistedRoles = new StringBuilder();
                var whitelistedUsers = new StringBuilder();

                if (data.RoleWhitelist.Count < 1) 
                    whitelistedRoles.AppendFormat("None");
                else
                {
                    var deletedRoles = new List<ulong>();

                    foreach (var roleId in data.RoleWhitelist)
                    {
                        var role = Context.Guild.GetRole(roleId);

                        if (role != null) 
                            whitelistedRoles.AppendFormat("{0} {1} ", SystemUtilities.GetSeparator, role.Name);
                        else 
                            deletedRoles.Add(roleId);
                    }

                    foreach (var id in deletedRoles) 
                        data.RemoveWhitelistRole(id);
                }

                if (data.UserWhitelist.Count < 1)
                    whitelistedUsers.AppendFormat("None");
                else
                {
                    var deletedUsers = new List<ulong>();

                    foreach (var userId in data.UserWhitelist)
                    {
                        var user = await Context.Guild.GetUserAsync(userId);

                        if (user != null)
                            whitelistedUsers.AppendFormat("{0} {1}#{2} ", SystemUtilities.GetSeparator,
                            user.Username, user.DiscriminatorValue);
                        else
                            deletedUsers.Add(userId);
                    }

                    foreach (var id in deletedUsers)
                        data.RemoveWhitelistUser(id);
                }

                var inactiveRole = Context.Guild.GetRole(data.InactiveRoleId);

                var output = new StringBuilder()
                    .AppendLine("Inactivity Monitor Configuration")
                    .AppendLine()
                    .AppendFormat("Enabled            : {0}", data.IsEnabled ? "Yes" : "No").AppendLine()
                    .AppendFormat("Inactive Role      : {0}", inactiveRole != null ? inactiveRole.Name : "Not Set!").AppendLine()
                    .AppendFormat("Inactive Threshold : {0} days after last active", data.InactiveDaysThreshold).AppendLine()
                    .AppendFormat("Kick Threshold     : {0} days after last active (0 = Not Set!)", data.KickDaysThreshold).AppendLine()
                    .AppendFormat("Whitelisted Roles  : {0}", whitelistedRoles.ToString()).AppendLine()
                    .AppendFormat("Whitelisted Users  : {0}", whitelistedUsers.ToString()).AppendLine();

                await ReplyAsync(Format.Code(output.ToString()));
            }
        }
    }
}
